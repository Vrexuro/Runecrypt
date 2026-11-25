using UnityEngine;

namespace Runecrypt.Core
{
    // Kamera mengikuti pemain tanpa memperlihatkan area di luar ruangan (spec M2.6 §5.1).
    public static class CameraBounds
    {
        public static Vector2 Clamp(Vector2 target, Rect room, Vector2 halfView) =>
            new Vector2(Axis(target.x, room.xMin, room.xMax, halfView.x), Axis(target.y, room.yMin, room.yMax, halfView.y));

        // Ruangan yang lebih sempit dari tampilan di satu sumbu: kamera diam di tengah ruangan pada sumbu itu.
        static float Axis(float t, float min, float max, float half) =>
            max - min <= 2f * half ? (min + max) * 0.5f : Mathf.Clamp(t, min + half, max - half);

        public static Vector2 SnapToPixel(Vector2 p, int ppu) =>
            new Vector2(Mathf.Round(p.x * ppu) / ppu, Mathf.Round(p.y * ppu) / ppu);
    }
}
