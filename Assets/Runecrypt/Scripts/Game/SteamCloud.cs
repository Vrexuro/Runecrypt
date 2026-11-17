using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Uap (GDD §5.1): awan yang melukai dan membutakan musuh di dalamnya. Tidak melukai pemain.
    public class SteamCloud : MonoBehaviour, IReactionEffect
    {
        [SerializeField] LightSourceBehaviour lightSource;

        ReactionDefinition def;
        float endTime, nextTick;
        int enemyMask;

        public void Init(ReactionDefinition d)
        {
            def = d;
            endTime = Time.time + d.duration;
            nextTick = Time.time;
            enemyMask = LayerMask.GetMask("Enemies");
            if (lightSource != null) lightSource.Radius = d.radius;
        }

        void Update()
        {
            if (def == null) return;
            float now = Time.time;
            while (nextTick <= now && nextTick <= endTime)
            {
                Tick(now);
                nextTick += def.tickInterval;
            }
            if (now >= endTime) Destroy(gameObject);
        }

        void Tick(float now)
        {
            Vector2 pos = transform.position;
            foreach (var hit in Physics2D.OverlapCircleAll(pos, def.radius, enemyMask))
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                if (enemy == null) continue;
                enemy.TakeHit(new HitInfo(def.damagePerTick, Element.None, false, pos));
                enemy.Blind(now + def.blindDuration);
            }
        }
    }
}
