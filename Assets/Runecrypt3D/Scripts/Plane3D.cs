using UnityEngine;

namespace Runecrypt.Game3D
{
    // Bidang permainan 2D (x, y) dipetakan ke lantai 3D (x, z), supaya logika Runecrypt.Core dipakai ulang.
    public static class Plane3D
    {
        public static Vector3 ToWorld(Vector2 p, float height = 0f) => new Vector3(p.x, height, p.y);
        public static Vector2 ToPlane(Vector3 w) => new Vector2(w.x, w.z);

        // Titik potong sinar dengan lantai y = floorY; false kalau sinar sejajar atau menjauh dari lantai.
        public static bool RayToFloor(Ray ray, float floorY, out Vector2 point)
        {
            point = default;
            float dy = ray.direction.y;
            if (Mathf.Abs(dy) < 1e-6f) return false;
            float t = (floorY - ray.origin.y) / dy;
            if (t <= 0f) return false;
            point = ToPlane(ray.origin + ray.direction * t);
            return true;
        }
    }
}
