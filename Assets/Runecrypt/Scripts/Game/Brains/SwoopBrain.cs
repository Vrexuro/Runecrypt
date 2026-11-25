using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Pelita Sesat: melayang, ancang-ancang dengan mata membesar (telegraph), menukik, mundur.
    // Durasi keputusan Claude, disetel saat playtest.
    [RequireComponent(typeof(Enemy))]
    public class SwoopBrain : MonoBehaviour, IEnemyBrain
    {
        [SerializeField] float hover = 1.4f, windup = 0.35f, dive = 0.45f, retreat = 0.6f;
        [SerializeField] float diveSpeed = 3f, retreatSpeed = 0.8f;
        [Tooltip("Mata (unlit, sorting order 10) dan sprite mata membesar saat ancang-ancang. Diisi builder M3a.")]
        [SerializeField] SpriteRenderer eyes;
        [SerializeField] Sprite windupEyes;

        Enemy enemy;
        SwoopCycle cycle;
        Sprite normalEyes;

        internal SwoopPhase Phase => cycle.Phase;

        void Awake()
        {
            enemy = GetComponent<Enemy>();
            cycle = new SwoopCycle(hover, windup, dive, retreat, diveSpeed, retreatSpeed);
            cycle.Reset(Time.time);
            if (eyes != null) normalEyes = eyes.sprite;
        }

        void Update()
        {
            // Buta atau stun: tidak menukik, dan siklus mulai lagi dari melayang setelahnya.
            if (enemy.IsStunnedOrBlind) cycle.Reset(Time.time);
            if (eyes != null && windupEyes != null)
                eyes.sprite = cycle.Phase == SwoopPhase.Windup ? windupEyes : normalEyes;
        }

        public Vector2 Steer(in EnemyContext c) => c.PlayerAlive ? cycle.Tick(c.Now, c.ToPlayer) : Vector2.zero;
    }
}
