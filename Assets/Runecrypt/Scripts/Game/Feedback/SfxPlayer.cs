using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Pool AudioSource 2D dengan batas suara bersamaan. Tanpa AudioMixer sampai M5 (aset .mixer tidak bisa dibuat lewat script).
    public class SfxPlayer : MonoBehaviour
    {
        const int Slots = 12;
        public static float MasterVolume = 1f;

        readonly VoiceLimiter limiter = new VoiceLimiter(Slots, 4);
        AudioSource[] sources;

        void Awake()
        {
            sources = new AudioSource[Slots];
            for (int i = 0; i < Slots; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.spatialBlend = 0f;
                sources[i] = s;
            }
        }

        public void Play(AudioClip[] clips, float volume, float pitchJitter)
        {
            if (clips == null || clips.Length == 0) return;
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip == null) return;
            float pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            int slot = limiter.Acquire(clip.GetInstanceID(), Time.unscaledTime, clip.length / pitch);
            if (slot < 0) return; // salinan serempak dari klip yang sama
            var src = sources[slot];
            src.Stop();
            src.clip = clip;
            src.pitch = pitch;
            src.volume = volume * MasterVolume;
            src.Play();
        }
    }
}
