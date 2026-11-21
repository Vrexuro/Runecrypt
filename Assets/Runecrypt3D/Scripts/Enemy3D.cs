using System;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;

namespace Runecrypt.Game3D
{
    // Musuh 3D berbasis EnemyDefinition yang sama dengan versi 2D. Aturan cahaya GDD §5.5 dari Core:
    // terang = damage +25%, gelap = kecepatan +20%. Kedipan putih saat kena; animasi jalan dari Locomotion3D.
    [RequireComponent(typeof(Rigidbody))]
    public class Enemy3D : MonoBehaviour, IDamageable
    {
        [SerializeField] EnemyDefinition definition;
        [SerializeField] LightRules lightRules;
        [SerializeField] Renderer[] flashRenderers;

        const float FlashTime = 0.08f;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public static event Action<Enemy3D> AnyDied;
        public float CurrentHp => health?.Current ?? 0f;
        public bool IsLit => LightFieldHost3D.Field.IsLit(Plane3D.ToPlane(transform.position));

        Rigidbody body;
        Health health;
        MaterialPropertyBlock flashBlock;
        float flashUntil, nextContact;
        bool flashing;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            if (definition != null) BuildHealth();
        }

        internal void Init(EnemyDefinition def, LightRules rules)
        {
            definition = def;
            lightRules = rules;
            BuildHealth();
        }

        void BuildHealth()
        {
            health = new Health(definition.maxHp);
            health.Died += Die;
        }

        void FixedUpdate()
        {
            if (health == null || health.IsDead) return;
            var player = Player3D.Instance;
            Vector2 dir = Vector2.zero;
            if (player != null && !player.Health.IsDead)
                dir = (Plane3D.ToPlane(player.transform.position) - Plane3D.ToPlane(transform.position)).normalized;
            float speed = definition.moveSpeed * lightRules.SpeedMultiplier(IsLit);
            body.linearVelocity = Plane3D.ToWorld(dir * speed);
            if (dir != Vector2.zero) transform.rotation = Quaternion.LookRotation(Plane3D.ToWorld(dir), Vector3.up);
        }

        void Update()
        {
            if (flashing && Time.time >= flashUntil) SetFlash(false);
        }

        public void TakeHit(HitInfo hit)
        {
            if (health == null || health.IsDead) return;
            SetFlash(true);
            flashUntil = Time.time + FlashTime;
            health.TakeDamage(lightRules.ApplyDamage(hit.Damage, IsLit));
        }

        void OnCollisionStay(Collision c)
        {
            if (health == null || health.IsDead || Time.time < nextContact) return;
            var player = c.collider.GetComponentInParent<Player3D>();
            if (player == null) return;
            player.TakeHit(new HitInfo(definition.contactDamage, definition.element, false, Plane3D.ToPlane(transform.position)));
            nextContact = Time.time + definition.contactInterval;
        }

        void SetFlash(bool on)
        {
            flashing = on;
            if (flashRenderers == null) return;
            flashBlock ??= new MaterialPropertyBlock();
            flashBlock.SetColor(BaseColor, Color.white * 3f);
            foreach (var r in flashRenderers) if (r != null) r.SetPropertyBlock(on ? flashBlock : null);
        }

        void Die()
        {
            AnyDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
