using UnityEngine;

namespace Runecrypt.Game
{
    // Bara di tumpukan abu: alpha berdenyut pelan dengan fase acak supaya tumpukan tidak berkedip serempak.
    [RequireComponent(typeof(SpriteRenderer))]
    public class EmberFlicker : MonoBehaviour
    {
        [SerializeField] float period = 1.2f;

        SpriteRenderer sr;
        float phase;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            phase = Random.value * Mathf.PI * 2f;
        }

        void Update()
        {
            var c = sr.color;
            c.a = 0.5f + 0.5f * Mathf.Sin(Time.time * 2f * Mathf.PI / period + phase);
            sr.color = c;
        }
    }
}
