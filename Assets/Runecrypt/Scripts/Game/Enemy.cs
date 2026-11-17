using System;
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
        [SerializeField] float knockbackSpeed = 4f;
        [SerializeField] float knockbackDamping = 12f;

        public static event Action<Enemy> AnyDied;
        public EnemyDefinition Definition => definition;

        Rigidbody2D body;
        Health health;
        readonly BurnStatus burn = new BurnStatus();
        float blindUntil, nextWanderChange, nextContact;
        Vector2 wanderDir, knock, lastHitPoint;

        bool IsLitNow => LightFieldHost.Field.IsLit(body.position);

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            health = new Health(definition.maxHp);
            health.Died += Die;
            if (burnLight != null) burnLight.SetActive(false);
        }

        void Update()
        {
            float now = Time.time;
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
            if (now < blindUntil)
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
                    ? ((Vector2)player.transform.position - body.position).normalized
                    : Vector2.zero;
            }
            body.linearVelocity = dir * definition.moveSpeed * lightRules.SpeedMultiplier(IsLitNow) + knock;
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
                body.linearVelocity += impulse; // langsung terasa di langkah fisika berikutnya
            }
            else
            {
                // Tick Uap: kedipan redup dan angka kecil, tanpa knockback (spec §5).
                if (hitFlash != null) hitFlash.Flash(Color.white, 0.35f, 0.08f);
                DamageNumbers.Show(applied, HitKind.Steam, pos);
            }
            health.TakeDamage(applied);
            if (hit.AppliesBurn && !health.IsDead) burn.Apply(Time.time);
        }

        public void Blind(float until) => blindUntil = Mathf.Max(blindUntil, until);

        void OnCollisionStay2D(Collision2D c)
        {
            // Musuh yang buta oleh Uap tidak menyerang (GDD §5.4).
            if (Time.time < nextContact || Time.time < blindUntil) return;
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
            Destroy(gameObject);
        }
    }
}
