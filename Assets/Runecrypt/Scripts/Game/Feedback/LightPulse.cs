using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Game
{
    // Denyut intensitas Light2D (tembakan). Radius tidak diubah, jadi aturan cahaya tidak terpengaruh.
    [RequireComponent(typeof(Light2D))]
    public class LightPulse : MonoBehaviour
    {
        Light2D light2D;
        float baseIntensity, multiplier = 1f, until;

        void Awake()
        {
            light2D = GetComponent<Light2D>();
            baseIntensity = light2D.intensity;
        }

        public void Pulse(float mult, float duration)
        {
            multiplier = mult;
            until = Time.time + duration;
        }

        void Update() => light2D.intensity = baseIntensity * (Time.time < until ? multiplier : 1f);
    }
}
