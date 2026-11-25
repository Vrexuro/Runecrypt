using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Biarawan Beku: menjaga jarak 4-6 tile dan menembakkan 3 serpih es ke pemain setiap 2,2 detik
    // kalau pemain dalam 9 tile. Tidak menembak saat buta atau stun (GDD §5.4). Angka keputusan Claude.
    [RequireComponent(typeof(Enemy))]
    public class RangedBrain : MonoBehaviour, IEnemyBrain
    {
        [SerializeField] float minRange = 4f, maxRange = 6f;
        [SerializeField] float fireInterval = 2.2f, fireRange = 9f;
        [SerializeField] int shardCount = 3;
        [SerializeField] float spreadDegrees = 15f;
        [SerializeField] EnemyShot shotPrefab;

        Enemy enemy;
        Cooldown fire;

        void Awake()
        {
            enemy = GetComponent<Enemy>();
            fire = new Cooldown(fireInterval);
            fire.Start(Time.time); // tembakan pertama setelah satu jeda, bukan tepat saat muncul
        }

        public Vector2 Steer(in EnemyContext c) =>
            c.PlayerAlive ? c.ToPlayer.normalized * RangeKeeper.Direction(c.ToPlayer.magnitude, minRange, maxRange) : Vector2.zero;

        void Update()
        {
            var player = PlayerController.Instance;
            float now = Time.time;
            if (shotPrefab == null || !enemy.enabled || enemy.IsStunnedOrBlind || player == null || player.Health.IsDead) return;
            Vector2 from = transform.position, toPlayer = (Vector2)player.transform.position - from;
            if (toPlayer.magnitude > fireRange || !fire.IsReady(now)) return;
            fire.Start(now);
            foreach (var dir in SpreadPattern.Directions(toPlayer, shardCount, spreadDegrees))
            {
                var shot = Instantiate(shotPrefab, from + dir * 0.5f, Quaternion.identity);
                shot.gameObject.SetActive(true);
                shot.Launch(dir);
            }
        }
    }
}
