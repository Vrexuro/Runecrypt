using System;

namespace Runecrypt.Core
{
    // Waktu selalu dikirim dari luar (bukan Time.time) supaya bisa diuji.
    public class Cooldown
    {
        const float Epsilon = 1e-4f; // 1f + 0.8f tidak persis 1.8f di float
        float readyAt = float.NegativeInfinity;

        public float Seconds { get; set; }

        public Cooldown(float seconds) => Seconds = seconds;

        public bool IsReady(float now) => now >= readyAt - Epsilon;
        public void Start(float now) => readyAt = now + Seconds;
        public float Remaining(float now) => Math.Max(0f, readyAt - now);
    }
}
