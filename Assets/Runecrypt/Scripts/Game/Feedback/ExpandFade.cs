using UnityEngine;

namespace Runecrypt.Game
{
    // Cincin uap: mengembang dari skala 0,5 ke targetScale sambil memudar, lalu hancur.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExpandFade : MonoBehaviour
    {
        [SerializeField] float duration = 0.2f;
        [SerializeField] float targetScale = 2.5f; // sprite 32 px = 2 unit; 2,5 = diameter 5 = radius Uap 2,5

        SpriteRenderer sr;
        Color baseColor;
        float born;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            baseColor = sr.color;
            born = Time.time;
            transform.localScale = Vector3.one * 0.5f;
        }

        void Update()
        {
            float t = Mathf.Clamp01((Time.time - born) / duration);
            transform.localScale = Vector3.one * Mathf.Lerp(0.5f, targetScale, t);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * (1f - t));
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
