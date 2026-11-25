using UnityEngine;

namespace Runecrypt.Core
{
    public enum SwoopPhase { Hover, Windup, Dive, Retreat }

    // Siklus Pelita Sesat (GDD §6.1, rencana M3a Task 3): melayang, ancang-ancang (arah dikunci),
    // menukik, mundur, lalu ulang. Mengembalikan gerak dalam satuan moveSpeed.
    public class SwoopCycle
    {
        readonly float[] durations;
        readonly float diveSpeed, retreatSpeed;
        float phaseStart;
        Vector2 lockedDir;

        public SwoopPhase Phase { get; private set; }

        public SwoopCycle(float hover, float windup, float dive, float retreat, float diveSpeed, float retreatSpeed)
        {
            const float Min = 0.01f; // durasi 0 dari Inspector akan membuat Tick berputar tanpa akhir
            durations = new[] { Mathf.Max(Min, hover), Mathf.Max(Min, windup), Mathf.Max(Min, dive), Mathf.Max(Min, retreat) };
            this.diveSpeed = diveSpeed;
            this.retreatSpeed = retreatSpeed;
        }

        public Vector2 Tick(float now, Vector2 toPlayer)
        {
            while (now >= phaseStart + durations[(int)Phase])
            {
                phaseStart += durations[(int)Phase];
                Phase = (SwoopPhase)(((int)Phase + 1) % durations.Length);
                if (Phase == SwoopPhase.Windup) lockedDir = toPlayer.normalized;
            }
            switch (Phase)
            {
                case SwoopPhase.Dive: return lockedDir * diveSpeed;
                case SwoopPhase.Retreat: return -toPlayer.normalized * retreatSpeed;
                default: return Vector2.zero;
            }
        }

        // Saat buta atau stun: siklus mulai lagi dari melayang.
        public void Reset(float now)
        {
            Phase = SwoopPhase.Hover;
            phaseStart = now;
        }
    }
}
