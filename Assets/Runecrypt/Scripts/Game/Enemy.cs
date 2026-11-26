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

        const float KillFlashTime = 0.06f;

        public static event Action<Enemy> AnyDied;
        static readonly List<Enemy> active = new List<Enemy>();
        // Musuh hidup yang bisa dikejar homing. Musuh yang mati dinonaktifkan, jadi otomatis keluar.
        public static IReadOnlyList<Enemy> Active => active;
        public EnemyDefinition Definition => definition;
        public float CurrentHp => health.Current;

        Rigidbody2D body;
        Health health;
        readonly BurnStatus burn = new BurnStatus();
        float blindUntil, stunUntil, nextWanderChange, nextContact, markedUntil;
        Vector2 wanderDir, knock, lastHitPoint;
        float bodyRadius = 0.4f;

        bool IsLitNow => LightFieldHost.Field.IsLit(body.position);

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            var circle = GetComponent<CircleCollider2D>();
            if (circle != null) bodyRadius = circle.radius * Mathf.Abs(transform.lossyScale.x);
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
                dir = player != null && !player.Health.IsDead
                    ? EnemyNav.Direction(body.position, player.transform.position, bodyRadius)
                    : Vector2.zero;
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
