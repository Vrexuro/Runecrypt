using UnityEngine;

namespace Runecrypt.Core
{
    public readonly struct AimResult
    {
        public readonly Vector2 Direction;
        public readonly bool AutoFire;

        public AimResult(Vector2 direction, bool autoFire)
        {
            Direction = direction;
            AutoFire = autoFire;
        }
    }

    // GDD §8.1: stik kanan > tombol panah (tembak otomatis) > mouse (tahan klik).
    public static class AimSelector
    {
        public static AimResult Select(Vector2 stick, Vector2 keys, Vector2 mouseWorld, Vector2 playerPos, Vector2 lastAim, float deadzone = 0.2f)
        {
            if (stick.magnitude > deadzone) return new AimResult(stick.normalized, true);
            if (keys.sqrMagnitude > 0f) return new AimResult(keys.normalized, true);
            var toMouse = mouseWorld - playerPos;
            return new AimResult(toMouse.magnitude < 0.01f ? lastAim : toMouse.normalized, false);
        }
    }
}
