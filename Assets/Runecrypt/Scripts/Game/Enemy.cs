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

        public static event Action<Enemy> AnyDied;
        public EnemyDefinition Definition => definition;

        Rigidbody2D body;
        Health health;
        readonly BurnStatus burn = new BurnStatus();
        float blindUntil, nextWanderChange, nextContact;
        Vector2 wanderDir;

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
            if (burnDamage > 0f) health.TakeDamage(lightRules.ApplyDamage(burnDamage, IsLitNow));
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
            body.linearVelocity = dir * definition.moveSpeed * lightRules.SpeedMultiplier(IsLitNow);
        }

        public void TakeHit(HitInfo hit)
        {
            if (health.IsDead) return;
            health.TakeDamage(lightRules.ApplyDamage(hit.Damage, IsLitNow));            if (hit.AppliesBurn && !health.IsDead) burn.Apply(Time.time);
        }

        public void Blind(float until) => blindUntil = Mathf.Max(blindUntil, until);

        void OnCollisionStay2D(Collision2D c)
        {
            if (Time.time < nextContact) return;
            var player = c.collider.GetComponent<PlayerController>();
            if (player == null) return;
            player.TakeHit(new HitInfo(definition.contactDamage, definition.element, false, body.position));
            nextContact = Time.time + definition.contactInterval;
        }

        void Die()
        {
            AnyDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
