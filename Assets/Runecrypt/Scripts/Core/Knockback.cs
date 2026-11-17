using UnityEngine;

namespace Runecrypt.Core
{
    // Knockback menekan kecepatan kejar sebanding kekuatan dorongan, jadi musuh benar-benar terlihat mundur
    // (review akhir M1.5). Tanpa ini dorongan hanya terasa sebagai musuh yang tertahan sesaat.
    public static class Knockback
    {
        public static Vector2 Combine(Vector2 chase, Vector2 knock, float fullStrength)
        {
            float suppress = fullStrength > 0f ? Mathf.Clamp01(knock.magnitude / fullStrength) : 0f;
            return chase * (1f - suppress) + knock;
        }
    }
}
