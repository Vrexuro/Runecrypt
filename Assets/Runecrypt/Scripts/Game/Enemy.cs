using System;
using System.Collections.Generic;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Musuh berbasis data. Aturan cahaya GDD §5.5: terang = damage +25%, gelap = kecepatan +20%.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] EnemyDefinition definition;
        [SerializeField] LightRules lightRules;
        [SerializeField] GameObject burnLight;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] GameObject homingMarker;
        [SerializeField] GameObject blindIcon;
        [SerializeField] GameObject stunIcon;
        [SerializeField] float knockbackSpeed = 4f;
        [SerializeField] float knockbackDamping = 12f;
        [Tooltip("Reaksi yang bisa dipicu musuh berelemen saat terkena api (M3a).")]
        [SerializeField] List<ReactionDefinition> reactionBook = new List<ReactionDefinition>();

        const float KillFlashTime = 0.06f;
        const float EmptyChargeAlpha = 0.6f; // keputusan Claude: tubuh memudar selama muatan kosong

        public static event Action<Enemy> AnyDied;
        static readonly List<Enemy> active = new List<Enemy>();
        // Musuh hidup yang bisa dikejar homing. Musuh yang mati dinonaktifkan, jadi otomatis keluar.
        public static IReadOnlyList<Enemy> Active => active;
        public EnemyDefinition Definition => definition;
        public float CurrentHp => health.Current;

        Rigidbody2D body;
        Health health;
        IEnemyBrain brain;
        Cooldown charge;
        SpriteRenderer bodySprite;
        readonly BurnStatus burn = new BurnStatus();
        float blindUntil, stunUntil, nextWanderChange, nextContact, markedUntil;
        Vector2 wanderDir, knock, lastHitPoint;

        bool IsLitNow => LightFieldHost.Field.IsLit(body.position);
        // Otak yang menyerang dari jauh menahan serangannya selama ini (GDD §5.4).
        internal bool IsStunnedOrBlind => Time.time < stunUntil || Time.time < blindUntil;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            brain = GetComponent<IEnemyBrain>();
            bodySprite = GetComponent<SpriteRenderer>();
            charge = new Cooldown(definition.chargeCooldown);
            health = new Health(definition.maxHp);
            health.Died += Die;
            if (burnLight != null) burnLight.SetActive(false);
        }

        void OnEnable() => active.Add(this);
        void OnDisable() => active.Remove(this);

        // Penanda bara pada musuh yang sedang dikejar homing (GDD §5.5 aturan 2).
        public void MarkTargeted(float now) => markedUntil = now + 0.1f;

        void Update()
        {
            float now = Time.time;
            SetShown(homingMarker, now < markedUntil);
            SetShown(blindIcon, now < blindUntil);
            SetShown(stunIcon, now < stunUntil);
            float burnDamage = burn.Tick(now);
            if (burnDamage > 0f)
            {
                float applied = lightRules.ApplyDamage(burnDamage, IsLitNow);
                DamageNumbers.Show(applied, HitKind.Burn, body.position);
                health.TakeDamage(applied);
            }
            if (bodySprite != null && definition.element != Element.None)
            {
                float a = charge.IsReady(now) ? 1f : EmptyChargeAlpha;
                if (!Mathf.Approximately(bodySprite.color.a, a)) bodySprite.color = new Color(1f, 1f, 1f, a);
            }
            bool burning = burn.IsActive(now) && !health.IsDead;
            if (burnLight != null && burnLight.activeSelf != burning) burnLight.SetActive(burning);
        }

        void FixedUpdate()
        {
            float now = Time.time;
            Vector2 dir;
            if (now < stunUntil) dir = Vector2.zero;
            else if (now < blindUntil)
            {
                if (now >= nextWanderChange)
                {
                    wanderDir = UnityEngine.Random.insideUnitCircle.normalized;
                    nextWanderChange = now + 0.5f;
                }
                dir = wanderDir;
            }
            else
            {
                var player = PlayerController.Instance;
                bool alive = player != null && !player.Health.IsDead;
                Vector2 toPlayer = alive ? (Vector2)player.transform.position - body.position : Vector2.zero;
                // Prefab tanpa otak tetap mengejar seperti sebelum M3a.
                dir = brain != null
                    ? brain.Steer(new EnemyContext { Position = body.position, ToPlayer = toPlayer, PlayerAlive = alive, Now = now, Lit = IsLitNow })
                    : (alive ? toPlayer.normalized : Vector2.zero);
            }
            Vector2 chase = dir * definition.moveSpeed * lightRules.SpeedMultiplier(IsLitNow);
            body.linearVelocity = Knockback.Combine(chase, knock, knockbackSpeed);
            knock *= Mathf.Exp(-knockbackDamping * Time.fixedDeltaTime);
        }

        public void TakeHit(HitInfo hit)
        {
            if (health.IsDead) return;
            bool lit = IsLitNow;
            float applied = lightRules.ApplyDamage(hit.Damage, lit);
            Vector2 pos = body.position;
            lastHitPoint = hit.Point;
            if (hit.Element == Element.Fire)
            {
                TryReact(hit.Element, pos);
                FeedbackPlayer.Play(lit ? definition.hitLit : definition.hitDark, pos, hitFlash);
                DamageNumbers.Show(applied, lit ? HitKind.Lit : HitKind.Dark, pos);
                Vector2 impulse = (pos - hit.Point).normalized * knockbackSpeed;
                knock += impulse;
                body.linearVelocity += impulse; // untuk langkah fisika sebelum FixedUpdate berikutnya (dan test Physics2D.Simulate)
            }
            else
            {
                // Tick Uap: kedipan redup dan angka kecil, tanpa knockback (spec §5).
                if (hitFlash != null) hitFlash.Flash(Color.white, 0.35f, 0.08f);
                DamageNumbers.Show(applied, HitKind.Steam, pos);
            }
            health.TakeDamage(applied);
            if (hit.AppliesBurn && !health.IsDead) burn.Apply(Time.time, hit.BurnDuration, hit.BurnDamagePerTick);
        }

        // Muatan elemen (GDD §5.1): musuh berelemen memicu reaksi di posisinya, lalu kosong selama chargeCooldown.
        // Dipanggil sebelum damage, jadi pukulan yang membunuh tetap memicu reaksi.
        void TryReact(Element incoming, Vector2 pos)
        {
            float now = Time.time;
            if (definition.element == Element.None || !charge.IsReady(now)) return;
            var def = ReactionResolver.Find(incoming, definition.element, reactionBook);
            if (def == null || def.effectPrefab == null) return;
            charge.Start(now); // sebelum spawn: tembakan lain di frame yang sama tidak memicu dua kali
            ReactionSpawner.Spawn(def, pos);
        }

        static void SetShown(GameObject go, bool shown)
        {
            if (go != null && go.activeSelf != shown) go.SetActive(shown);
        }

        public void Blind(float until) => blindUntil = Mathf.Max(blindUntil, until);
        // Stun (GDD §5.4): diam sesaat dan tidak menyerang. Knockback tetap berlaku. Dipakai Ledakan Cahaya (M3).
        public void Stun(float until) => stunUntil = Mathf.Max(stunUntil, until);

        void OnCollisionStay2D(Collision2D c)
        {
            // Musuh yang buta oleh Uap tidak menyerang (GDD §5.4).
            if (Time.time < nextContact || Time.time < blindUntil || Time.time < stunUntil) return;
            var player = c.collider.GetComponent<PlayerController>();
            if (player == null) return;
            player.TakeHit(new HitInfo(definition.contactDamage, definition.element, false, body.position));
            nextContact = Time.time + definition.contactInterval;
        }

        void Die()
        {
            Vector2 pos = body.position;
            FeedbackPlayer.Play(definition.deathProfile, pos, hitFlash);
            if (definition.deathEffect != null)
            {
                var fx = Instantiate(definition.deathEffect, pos, Quaternion.identity);
                var death = fx.GetComponent<DeathEffect>();
                if (death != null) death.Init(pos - lastHitPoint);
            }
            if (definition.remains != null) RemainsRegistry.Add(Instantiate(definition.remains, pos, Quaternion.identity));
            // Setelah reaksi pukulan terakhir (TryReact dipanggil sebelum damage), jadi benda ini tidak ikut bereaksi.
            if (definition.dropOnDeath != null) Instantiate(definition.dropOnDeath, pos, Quaternion.identity);
            AnyDied?.Invoke(this);

            // Tubuh bertahan sesaat supaya kedipan pukulan terakhir terlihat (spec §5.1 langkah 1), tapi sudah
            // tidak bisa disentuh atau ditembak. Waktu berskala, jadi ikut tertahan selama hit-stop.
            enabled = false;
            body.simulated = false;
            foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
            float linger = KillFlashTime;
            if (definition.deathProfile != null) linger = Mathf.Max(linger, definition.deathProfile.flashDuration);
            Destroy(gameObject, linger);
        }
    }
}
