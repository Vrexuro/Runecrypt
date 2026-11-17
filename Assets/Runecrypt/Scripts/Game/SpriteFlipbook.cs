using UnityEngine;

namespace Runecrypt.Game
{
    // Animasi loop sederhana untuk prototipe. Semua flipbook memakai Time.time, jadi badan dan mata sinkron.
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float fps = 6f;
        SpriteRenderer sr;

        void Awake() => sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (frames == null || frames.Length == 0) return;
            sr.sprite = frames[(int)(Time.time * fps) % frames.Length];
        }
    }
}
