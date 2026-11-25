using UnityEngine;

namespace Runecrypt.Game
{
    // Tulang Penjaga: berjalan lurus mengejar pemain.
    public class ChaseBrain : MonoBehaviour, IEnemyBrain
    {
        public Vector2 Steer(in EnemyContext c) => c.PlayerAlive ? c.ToPlayer.normalized : Vector2.zero;
    }
}
