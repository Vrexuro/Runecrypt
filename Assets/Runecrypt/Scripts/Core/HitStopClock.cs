using System;

namespace Runecrypt.Core
{
    // Hit-stop: permintaan yang tumpang tindih diambil yang terpanjang, tidak dijumlahkan.
    public class HitStopClock
    {
        float endsAt = float.NegativeInfinity;

        public void Request(float now, float duration) => endsAt = Math.Max(endsAt, now + duration);

        public bool IsStopped(float now) => now < endsAt;

        // Jeda game (layar Tumbang) selalu menang, jadi hit-stop yang selesai tidak membatalkannya.
        public float TimeScaleAt(float now, bool paused) => paused || IsStopped(now) ? 0f : 1f;
    }
}
