using UnityEngine;

namespace Runecrypt.Core
{
    public readonly struct HitInfo
    {
        public readonly float Damage;
        public readonly Element Element;
        public readonly bool AppliesBurn;
        public readonly Vector2 Point;

        public HitInfo(float damage, Element element, bool appliesBurn, Vector2 point)
        {
            Damage = damage;
            Element = element;
            AppliesBurn = appliesBurn;
            Point = point;
        }
    }
}
