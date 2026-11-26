using UnityEngine;

namespace Runecrypt.Game
{
    // Tulang Penjaga: mengejar pemain, memutari penghalang lewat EnemyNav.
    public class ChaseBrain : MonoBehaviour, IEnemyBrain
    {
        public Vector2 Steer(in EnemyContext c) => c.PlayerAlive ? c.ChaseDir : Vector2.zero;
    }
}
