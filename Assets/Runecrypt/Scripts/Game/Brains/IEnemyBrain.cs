using UnityEngine;

namespace Runecrypt.Game
{
    // Keadaan yang dilihat otak musuh setiap langkah fisika (rencana M3a Task 1).
    public struct EnemyContext
    {
        public Vector2 Position, ToPlayer;
        public bool PlayerAlive;
        public float Now;
        public bool Lit;
    }

    // Cara musuh bergerak. Stun dan Buta tetap diurus Enemy dan didahulukan.
    public interface IEnemyBrain
    {
        // Arah dan besar gerak dalam satuan moveSpeed (1 = kecepatan normal). Enemy mengalikan dengan moveSpeed dan aturan gelap.
        Vector2 Steer(in EnemyContext c);
    }
}
