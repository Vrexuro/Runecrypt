using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Serpih es Biarawan Beku: hanya melukai pemain, menembus musuh, hancur di dinding dan benda padat
    // (nisan, sarkofagus, pilar menahan tembakan sejak M2.5). Angka keputusan Claude, disetel saat playtest.
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyShot : MonoBehaviour
    {
        [SerializeField] float damage = 1f;
        [SerializeField] float speed = 6f;
        [SerializeField] float range = 9f;
        [SerializeField] Element element = Element.Ice;

        Rigidbody2D body;
        int blockMask;
        float traveled, castRadius;
        bool spent;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            blockMask = LayerMask.GetMask("Walls", "Props");
            var col = GetComponent<CircleCollider2D>();
            castRadius = col != null ? col.radius : 0.1f;
        }

        public void Launch(Vector2 direction) => body.linearVelocity = direction.normalized * speed;

        void FixedUpdate()
        {
            if (spent) return;
            Vector2 v = body.linearVelocity;
            float step = v.magnitude * Time.fixedDeltaTime;
            // Dicek dengan cast sepanjang langkah supaya serpih tidak menembus dinding tipis.
            if (Physics2D.CircleCast(body.position, castRadius, v, step, blockMask).collider != null) { Spend(); return; }
            traveled += step;
            if (traveled >= range) Spend();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (spent) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return; // musuh dan benda lain diabaikan
            player.TakeHit(new HitInfo(damage, element, false, body.position));
            Spend();
        }

        void Spend()
        {
            spent = true;
            Destroy(gameObject);
        }
    }
}
