using System;

namespace Runecrypt.Core
{
    // Guncangan berbasis trauma: kejadian menambah trauma 0–1, lalu turun linear. Offset = trauma² × maksimum.
    public class ShakeTrauma
    {
        readonly float decayPerSecond, maxPixels;

        public ShakeTrauma(float decayPerSecond = 1.5f, float maxPixels = 3f)
        {
            this.decayPerSecond = decayPerSecond;
            this.maxPixels = maxPixels;
        }

        public float Value { get; private set; }

        public float OffsetPixels => Value * Value * maxPixels;

        public void Add(float amount) => Value = Math.Clamp(Value + amount, 0f, 1f);

        public void Tick(float dt) => Value = Math.Max(0f, Value - decayPerSecond * dt);
    }
}
