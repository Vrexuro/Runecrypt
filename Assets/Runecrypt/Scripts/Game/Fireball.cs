using System.Collections.Generic;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Bola Api (GDD §4.1): damage 3, api, memberi Terbakar, jangkauan 7 tile.
    // Modifier relic (GDD §5.2) datang lewat ShotStats: pierce, bounce, homing, ukuran, cahaya.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Fireball : MonoBehaviour
    {
        [SerializeField] float damage = 3f;
        [SerializeField] float speed = 12f;
        [SerializeField] float range = 7f;

        const float HomingRange = 6f;
        static readonly List<Vector2> targetPoints = new List<Vector2>();

        public ShotStats Defaults => new ShotStats
        {
            Damage = damage, Speed = speed, Range = range, Size = 1f, LightScale = 1f,
            BurnDuration = 2f, BurnDamagePerTick = 1f, Count = 1,
        };

        Rigidbody2D body;
        ShotStats shot;
        LightSourceBehaviour ownLight;
        readonly HashSet<IDamageable> hits = new HashSet<IDamageable>();
        float traveled, castRadius;
        int pierceLeft, bounceLeft, wallMask;
        bool spent; // Destroy baru berlaku di akhir frame; tanpa ini satu bola api bisa kena beberapa sasaran

        public void Launch(Vector2 direction) => Launch(direction, Defaults);

        public void Launch(Vector2 direction, ShotStats s)
        {
            body = GetComponent<Rigidbody2D>();
            shot = s;
            pierceLeft = s.Pierce;
            bounceLeft = s.Bounce;
            wallMask = LayerMask.GetMask("Walls");
            transform.localScale = Vector3.one * s.Size;
            var col = GetComponent<CircleCollider2D>();
            castRadius = col != null ? col.radius * s.Size : 0.1f;
            foreach (var light in GetComponentsInChildren<LightSourceBehaviour>()) light.Radius *= s.LightScale;
            ownLight = GetComponentInChildren<LightSourceBehaviour>();
            body.linearVelocity = direction.normalized * s.Speed;
        }

        void FixedUpdate()
        {
            if (body == null || spent) return;
            float dt = Time.fixedDeltaTime;
            if (shot.HomingTurn > 0f) SteerToLitEnemy(dt);

            // Dinding dicek dengan cast sepanjang langkah ini supaya bola api cepat tidak menembus dinding.
            Vector2 v = body.linearVelocity;
            float step = v.magnitude * dt;
            var hit = Physics2D.CircleCast(body.position, castRadius, v, step, wallMask);
            if (hit.collider != null)
            {
                if (bounceLeft <= 0) { spent = true; Destroy(gameObject); return; }
                bounceLeft--;
                body.position = hit.centroid + hit.normal * 0.01f;
                body.linearVelocity = Vector2.Reflect(v, hit.normal);
            }

            // Jarak tempuh total, bukan jarak dari titik awal, karena bola api bisa memantul.
            traveled += step;
            if (traveled >= shot.Range) { spent = true; Destroy(gameObject); }
        }

        void SteerToLitEnemy(float dt)
        {
            var enemies = Enemy.Active;
            targetPoints.Clear();
            foreach (var e in enemies) targetPoints.Add(e.transform.position);
            // Cahaya bola api ini sendiri tidak dihitung (GDD §5.5 aturan 2).
            int i = TargetSelector.Nearest(body.position, targetPoints, HomingRange,
                p => LightFieldHost.Field.IsLit(p, ownLight));
            if (i < 0) return;
            body.linearVelocity = Homing.Steer(body.linearVelocity, targetPoints[i] - body.position, shot.HomingTurn * dt);
            enemies[i].MarkTargeted(Time.time);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (spent) return;
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target is PlayerController) return;
            if (!hits.Add(target)) return; // pierce: sasaran yang sama hanya sekali
            bool last = pierceLeft <= 0;
            if (last) spent = true;
            // Cahaya bola api ini sendiri tidak ikut membuat sasarannya Tersingkap (GDD §5.5), juga saat menembus.
            var lights = GetComponentsInChildren<LightSourceBehaviour>();
            foreach (var light in lights) light.enabled = false;
            target.TakeHit(new HitInfo(shot.Damage, Element.Fire, true, body.position, shot.BurnDuration, shot.BurnDamagePerTick));
            if (last) { Destroy(gameObject); return; }
            pierceLeft--;
            foreach (var light in lights) light.enabled = true;
        }
    }
}
