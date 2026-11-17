using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Guncangan kamera berbasis trauma, dibulatkan ke piksel (16 PPU) supaya pixel art tetap tajam.
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        const float PixelSize = 1f / 16f;

        readonly ShakeTrauma trauma = new ShakeTrauma();
        Vector3 basePos;
        Vector2 kick;

        // shortcut: posisi dasar diambil sekali, karena kamera M1 diam; ganti ke offset relatif saat kamera mulai mengikuti ruangan (M4)
        void Awake() => basePos = transform.position;

        public void Add(float amount) => trauma.Add(amount);

        public void Kick(Vector2 dir, int pixels) => kick = dir.normalized * pixels;

        void LateUpdate()
        {
            trauma.Tick(Time.unscaledDeltaTime);
            Vector2 off = Random.insideUnitCircle.normalized * trauma.OffsetPixels + kick;
            kick = Vector2.zero;
            off = new Vector2(Mathf.Round(off.x), Mathf.Round(off.y)) * PixelSize;
            transform.position = basePos + (Vector3)off;
        }
    }
}
