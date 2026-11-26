using UnityEngine;

namespace Runecrypt.Core
{
    // Arah hadap pemain saat menyerang. Sprite mage menghadap kanan, jadi flip = hadap kiri.
    public static class Facing
    {
        // Serangan yang hampir tegak lurus ke atas atau ke bawah tidak membalik badan, supaya sprite tidak berkedip.
        const float VerticalDeadzone = 0.05f;

        public static bool FlipForAttack(Vector2 attackDirection, bool currentFlip)
        {
            if (Mathf.Abs(attackDirection.x) < VerticalDeadzone) return currentFlip;
            return attackDirection.x < 0f;
        }
    }
}
