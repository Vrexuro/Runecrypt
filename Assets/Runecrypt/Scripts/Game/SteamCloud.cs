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

        // Setelah relic (Mahkota Asap: +50%), tanpa pemain pakai nilai definisi.
        internal float Radius { get; private set; }
        internal float Duration { get; private set; }
        int enemyMask;

        public void Init(ReactionDefinition d)
        {
            def = d;
            var stats = PlayerController.Instance != null ? PlayerController.Instance.Stats : null;
            Radius = stats != null ? stats.Get(Stat.SteamRadius, d.radius) : d.radius;
            Duration = stats != null ? stats.Get(Stat.SteamDuration, d.duration) : d.duration;
            if (d.radius > 0f) transform.localScale *= Radius / d.radius; // visual awan ikut radius
            endTime = Time.time + Duration;
            nextTick = Time.time;
            enemyMask = LayerMask.GetMask("Enemies");
            if (lightSource != null) lightSource.Radius = Radius;
            FeedbackPlayer.Play(d.spawnProfile, transform.position);
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
            foreach (var hit in Physics2D.OverlapCircleAll(pos, Radius, enemyMask))
            {
                var enemy = hit.GetComponentInParent<Enemy>();
                if (enemy == null) continue;
                enemy.TakeHit(new HitInfo(def.damagePerTick, Element.None, false, pos));
                enemy.Blind(now + def.blindDuration);
            }
        }
    }
}
