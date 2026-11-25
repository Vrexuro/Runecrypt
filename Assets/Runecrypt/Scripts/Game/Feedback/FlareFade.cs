using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Game
{
    // Kilatan cahaya visual (spec M2.6 §5.4): terang sesaat lalu padam dan hilang dalam ≤ 0,2 detik.
    // Sengaja tanpa LightSourceBehaviour, jadi tidak membuat musuh Tersingkap (pengecualian GDD §5.5).
    [RequireComponent(typeof(Light2D))]
    public class FlareFade : MonoBehaviour
    {
        [SerializeField] float duration = 0.15f;

        Light2D flare;
        float start, born;

        void Awake()
        {
            flare = GetComponent<Light2D>();
            start = flare.intensity;
            born = Time.time;
        }

        void Update()
        {
            float t = (Time.time - born) / duration;
            flare.intensity = start * Mathf.Clamp01(1f - t);
            if (t < 1f) return;
            flare.enabled = false; // objek induk (efek) dihancurkan oleh pemiliknya
            enabled = false;
        }
    }
}
