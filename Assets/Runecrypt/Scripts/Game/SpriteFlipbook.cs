using UnityEngine;

namespace Runecrypt.Game
{
    // Animasi sederhana. Mode loop memakai Time.time, jadi badan dan mata sinkron; mode playOnce mulai dari saat muncul.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 6f;
        [Tooltip("Putar sekali dari saat muncul lalu hapus objeknya (efek mati M3a).")]
        [SerializeField] bool playOnce;
        SpriteRenderer sr;
        float born;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            born = Time.time;
        }

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            if (!playOnce)
            {
                sr.sprite = frames[(int)(Time.time * fps) % frames.Length];
                return;
            }
            int i = (int)((Time.time - born) * fps);
            if (i >= frames.Length) { Destroy(gameObject); return; }
            sr.sprite = frames[i];
        }
    }
}
