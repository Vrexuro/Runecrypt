using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Satu-satunya penulis Time.timeScale selama permainan berjalan. Jeda Tumbang (GamePause) selalu menang.
    public class HitStop : MonoBehaviour
    {
        readonly HitStopClock clock = new HitStopClock();

        public void Request(float duration)
        {
            if (GamePause.IsPaused) return;
            clock.Request(Time.unscaledTime, duration);
            Time.timeScale = 0f; // berhenti di frame ini juga, bukan frame berikutnya
        }

        void Update() => Time.timeScale = clock.TimeScaleAt(Time.unscaledTime, GamePause.IsPaused);
    }
}
