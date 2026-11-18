using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runecrypt.Core
{
    // Bara Kembar (GDD §5.2): proyektil menyebar simetris terhadap arah bidik.
    public static class SpreadPattern
    {
        public static Vector2[] Directions(Vector2 aim, int count, float spacingDegrees)
        {
            count = Math.Max(1, count);
            var baseDir = aim.sqrMagnitude > 0f ? aim.normalized : Vector2.right;
            var dirs = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float deg = (i - (count - 1) * 0.5f) * spacingDegrees;
                dirs[i] = (Vector2)(Quaternion.Euler(0f, 0f, deg) * baseDir);
            }
            return dirs;
        }
    }

    // Homing hanya boleh memilih sasaran terang (GDD §5.5 aturan 2).
    public static class TargetSelector
    {
        public static int Nearest(Vector2 from, IReadOnlyList<Vector2> points, float maxRange, Func<Vector2, bool> isLit)
        {
            int best = -1;
            float bestSq = maxRange * maxRange;
            for (int i = 0; i < points.Count; i++)
            {
                float sq = (points[i] - from).sqrMagnitude;
                if (sq > bestSq || !isLit(points[i])) continue;
                best = i;
                bestSq = sq;
            }
            return best;
        }
    }

    public static class Homing
    {
        // Memutar arah gerak ke sasaran, paling banyak maxDegrees, dengan kecepatan tetap.
        public static Vector2 Steer(Vector2 velocity, Vector2 toTarget, float maxDegrees)
        {
            if (velocity.sqrMagnitude <= 0f || toTarget.sqrMagnitude <= 0f) return velocity;
            float delta = Vector2.SignedAngle(velocity, toTarget);
            float turn = Mathf.Clamp(delta, -maxDegrees, maxDegrees);
            return Quaternion.Euler(0f, 0f, turn) * velocity;
        }
    }
}
