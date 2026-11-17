using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Game
{
    // Kematian "abu dan tulang" (spec §5.1): serpihan meluncur lalu diam dan pudar, kepulan abu,
    // kilatan cahaya singkat, dan mata merah yang bertahan sesaat lalu padam.
    public class DeathEffect : MonoBehaviour
    {
        const float SlideTime = 0.3f, RestTime = 0.6f, FadeTime = 0.6f, Life = 1.5f;
        const float FlareTime = 0.15f, EyesHold = 0.3f, EyesFade = 0.1f, HopHeight = 0.12f;

        [SerializeField] SpriteRenderer[] shards;
        [SerializeField] ParticleSystem ashPuff;
        [SerializeField] Light2D flare;
        [SerializeField] SpriteRenderer eyes;
        [SerializeField] int minShards = 6;

        Vector2[] basePos, velocity;
        float[] spin;
        float born;

        public void Init(Vector2 awayDir)
        {
            born = Time.time;
            Vector2 dir = awayDir.sqrMagnitude > 1e-4f ? awayDir.normalized : Random.insideUnitCircle.normalized;
            int count = Random.Range(minShards, shards.Length + 1);
            basePos = new Vector2[shards.Length];
            velocity = new Vector2[shards.Length];
            spin = new float[shards.Length];
            for (int i = 0; i < shards.Length; i++)
            {
                shards[i].gameObject.SetActive(i < count);
                basePos[i] = shards[i].transform.position;
                float angle = Random.Range(-70f, 70f) * Mathf.Deg2Rad;
                Vector2 d = new Vector2(dir.x * Mathf.Cos(angle) - dir.y * Mathf.Sin(angle), dir.x * Mathf.Sin(angle) + dir.y * Mathf.Cos(angle));
                velocity[i] = d * Random.Range(2f, 4f);
                spin[i] = Random.Range(-720f, 720f);
            }
            if (ashPuff != null) ashPuff.Play();
            Destroy(gameObject, Life);
        }

        void Update()
        {
            if (velocity == null) return; // Init belum dipanggil
            float age = Time.time - born, dt = Time.deltaTime;
            float slide = Mathf.Clamp01(1f - age / SlideTime); // gesekan: kecepatan turun ke nol dalam 0,3 dtk
            float hop = age < SlideTime ? Mathf.Sin(Mathf.PI * age / SlideTime) * HopHeight : 0f;
            float shardAlpha = 1f - Mathf.Clamp01((age - SlideTime - RestTime) / FadeTime);
            for (int i = 0; i < shards.Length; i++)
            {
                var s = shards[i];
                if (!s.gameObject.activeSelf) continue;
                basePos[i] += velocity[i] * slide * dt;
                s.transform.position = basePos[i] + Vector2.up * hop;
                s.transform.Rotate(0f, 0f, spin[i] * slide * dt);
                var c = s.color; c.a = shardAlpha; s.color = c;
            }
            if (flare != null) flare.intensity = 1f - Mathf.Clamp01(age / FlareTime);
            if (eyes != null)
            {
                var c = eyes.color; c.a = 1f - Mathf.Clamp01((age - EyesHold) / EyesFade); eyes.color = c;
            }
        }
    }
}
