using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Memutar FeedbackProfile: kedipan, hit-stop, guncangan, partikel, dan SFX.
    // Semua pemanggil aman tanpa FeedbackPlayer di scene (test, scene baru).
    public class FeedbackPlayer : MonoBehaviour
    {
        public static FeedbackPlayer Instance { get; private set; }

        HitStop hitStop;
        CameraShake shake;
        SfxPlayer sfx;

        // Dicari saat dibutuhkan: Awake ini bisa jalan sebelum kamera terdaftar sebagai Camera.main.
        CameraShake Shake
        {
            get
            {
                if (shake == null && Camera.main != null) shake = Camera.main.GetComponent<CameraShake>();
                return shake;
            }
        }

        void Awake()
        {
            Instance = this;
            hitStop = GetComponent<HitStop>();
            sfx = GetComponent<SfxPlayer>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Play(FeedbackProfile p, Vector2 pos, HitFlash target = null)
        {
            if (p == null) return;
            if (target != null && p.flashDuration > 0f) target.Flash(p.flashColor, p.flashAlpha, p.flashDuration);
            var fp = Instance;
            if (fp == null) return;
            if (p.hitStop > 0f && fp.hitStop != null) fp.hitStop.Request(p.hitStop);
            if (p.shakeTrauma > 0f && fp.Shake != null) fp.Shake.Add(p.shakeTrauma);
            if (p.particlePrefab != null) Destroy(Instantiate(p.particlePrefab, pos, Quaternion.identity), 2f);
            if (fp.sfx != null) fp.sfx.Play(p.clips, p.volume, p.pitchJitter);
        }

        public static void Kick(Vector2 dir, int pixels)
        {
            var fp = Instance;
            if (fp != null && fp.Shake != null) fp.Shake.Kick(dir, pixels);
        }
    }
}
