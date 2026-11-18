using System.Collections.Generic;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Kobaran (GDD §4.1): cincin api di sekitar pemain. Dihitung sebagai api, jadi bisa memicu reaksi.
    public class BlazeRing : MonoBehaviour
    {
        [SerializeField] LightSourceBehaviour lightSource;
        // Cahaya sedikit lebih luas dan lebih lama dari ring, supaya musuh di tepinya tetap Tersingkap
        // untuk tembakan lanjutan (keputusan rencana M2).
        [SerializeField] float lightRadius = 3.5f;
        [SerializeField] float lightTime = 0.5f;
        [SerializeField] float lifetime = 0.6f;

        static readonly HashSet<IDamageable> struck = new HashSet<IDamageable>();

        public void Init(float damage, float radius, float burnDuration, float burnDamagePerTick)
        {
            Vector2 pos = transform.position;
            // Cahaya menyala lebih dulu, jadi musuh di dalam ring menerima bonus Tersingkap dari pukulan ini.
            if (lightSource != null)
            {
                lightSource.Radius = lightRadius;
                lightSource.gameObject.SetActive(true);
                Destroy(lightSource.gameObject, lightTime);
            }

            struck.Clear();
            foreach (var col in Physics2D.OverlapCircleAll(pos, radius))
            {
                var target = col.GetComponentInParent<IDamageable>();
                // Musuh dengan beberapa collider hanya dipukul sekali.
                if (target == null || target is PlayerController || !struck.Add(target)) continue;
                target.TakeHit(new HitInfo(damage, Element.Fire, true, pos, burnDuration, burnDamagePerTick));
            }
            Destroy(gameObject, lifetime);
        }
    }
}
