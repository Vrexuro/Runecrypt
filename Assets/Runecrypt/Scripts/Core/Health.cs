using System;

namespace Runecrypt.Core
{
    public class Health
    {
        public float Max { get; }
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
    }
}
