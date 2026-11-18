using System;

namespace Runecrypt.Core
{
    public class Health
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;
        public event Action Died;

        public Health(float max)
        {
            Max = max;
            Current = max;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Current = Math.Max(0f, Current - amount);
            if (IsDead) Died?.Invoke();
        }

        // Relic seperti Jantung Tungku (GDD §5.2). HP sekarang turun kalau melebihi batas baru.
        public void SetMax(float max)
        {
            if (IsDead) return;
            Max = Math.Max(1f, max);
            Current = Math.Min(Current, Max);
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead) return;
            Current = Math.Min(Max, Current + amount);
        }
    }
}
