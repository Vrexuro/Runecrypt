using UnityEngine;

namespace Runecrypt.Game3D
{
    // Laju klip Move dari kecepatan gerak (Docs/specs/2026-11-21-3d-animasi-jalan-design.md).
    public static class LocomotionMath
    {
        // speed / v0 dibatasi [minMul, maxMul]; negatif (klip terbalik) kalau boleh dan gerak jelas ke belakang tubuh.
        public static float SpeedMul(float speed, float forwardSpeed, float v0, float minMul, float maxMul, bool allowReverse)
        {
            float mul = Mathf.Clamp(speed / v0, minMul, maxMul);
            return allowReverse && forwardSpeed < -0.3f * speed ? -mul : mul;
        }
    }
}
