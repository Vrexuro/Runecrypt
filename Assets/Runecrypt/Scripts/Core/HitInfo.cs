using UnityEngine;

namespace Runecrypt.Core
{
    public readonly struct HitInfo
    {
        public readonly float Damage;
        public readonly Element Element;
        public readonly bool AppliesBurn;
        public readonly Vector2 Point;
        public readonly float BurnDuration;
        public readonly float BurnDamagePerTick;

        public HitInfo(float damage, Element element, bool appliesBurn, Vector2 point,
            float burnDuration = 2f, float burnDamagePerTick = 1f)
        {
            BurnDuration = burnDuration;
            BurnDamagePerTick = burnDamagePerTick;
            Damage = damage;
            Element = element;
            AppliesBurn = appliesBurn;
            Point = point;
        }
    }
}
