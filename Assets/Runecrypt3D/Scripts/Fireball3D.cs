using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;

namespace Runecrypt.Game3D
{
    // Bola Api 3D (GDD §4.1): damage 3, api, memberi Terbakar, kecepatan 12, jangkauan 7 tile, terbang setinggi pinggang.
    // Gerak manual + SphereCast sepanjang langkah, supaya bola api cepat tidak menembus dinding atau musuh.
    public class Fireball3D : MonoBehaviour
    {
        [SerializeField] float damage = 3f;
        [SerializeField] float speed = 12f;
        [SerializeField] float range = 7f;
        [SerializeField] float radius = 0.15f;

        internal float Speed { get => speed; set => speed = value; }

        Vector3 dir;
        float traveled;
        int mask, wallLayer;
        bool spent;

        public void Launch(Vector2 direction)
        {
            dir = Plane3D.ToWorld(direction.normalized);
            mask = LayerMask.GetMask("Walls", "Enemies");
            wallLayer = LayerMask.NameToLayer("Walls");
        }

        void FixedUpdate()
        {
            if (spent || dir == Vector3.zero) return;
            float step = speed * Time.fixedDeltaTime;
            if (Physics.SphereCast(transform.position, radius, dir, out var hit, step, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject.layer != wallLayer)
                {
                    var target = hit.collider.GetComponentInParent<IDamageable>();
                    if (target != null && !(target is Player3D))
                        target.TakeHit(new HitInfo(damage, Element.Fire, true, Plane3D.ToPlane(hit.point)));
                }
                Expire();
                return;
            }
            transform.position += dir * step;
            traveled += step;
            if (traveled >= range) Expire();
        }

        void Expire()
        {
            spent = true;
            Destroy(gameObject);
        }
    }
}
