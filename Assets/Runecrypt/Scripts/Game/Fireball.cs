using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Bola Api (GDD §4.1): damage 3, api, memberi Terbakar, jangkauan 7 tile.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Fireball : MonoBehaviour
    {
        [SerializeField] float damage = 3f;
        [SerializeField] float speed = 12f;
        [SerializeField] float range = 7f;

        Rigidbody2D body;
        Vector2 origin;

        public void Launch(Vector2 direction)
        {
            body = GetComponent<Rigidbody2D>();
            origin = body.position;
            body.linearVelocity = direction.normalized * speed;
        }

        void FixedUpdate()
        {
            if ((body.position - origin).sqrMagnitude >= range * range) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Walls")) { Destroy(gameObject); return; }
            var target = other.GetComponentInParent<IDamageable>();
            if (target == null || target is PlayerController) return;
            // Cahaya bola api ini sendiri tidak ikut membuat sasarannya Tersingkap (GDD §5.5).
            foreach (var light in GetComponentsInChildren<LightSourceBehaviour>()) light.enabled = false;
            target.TakeHit(new HitInfo(damage, Element.Fire, true, body.position));
            Destroy(gameObject);
        }
    }
}
