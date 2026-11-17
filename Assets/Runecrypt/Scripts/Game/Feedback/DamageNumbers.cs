using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Angka damage dari sprite digit 3x5 (spec §5.3). Pool 40; kalau habis, yang tertua dipakai ulang.
    public class DamageNumbers : MonoBehaviour
    {
        const int PoolSize = 40, MaxDigits = 3;
        const float Life = 0.6f, FadeTime = 0.2f, Gravity = 9f, Rise = 3f, Spread = 1f;
        const float DigitWidth = 4f / 16f, SpawnHeight = 0.4f;

        [SerializeField] Sprite[] digits = new Sprite[10];
        [SerializeField] Material material;
        [SerializeField] int sortingOrder = 50;

        public static DamageNumbers Instance { get; private set; }

        class Number
        {
            public Transform root;
            public SpriteRenderer[] glyphs;
            public Vector2 velocity;
            public Color color;
            public float born;
            public bool active;
        }

        Number[] pool;
        int next;

        void Awake()
        {
            Instance = this;
            pool = new Number[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                var root = new GameObject("Number").transform;
                root.SetParent(transform, false);
                var glyphs = new SpriteRenderer[MaxDigits];
                for (int d = 0; d < MaxDigits; d++)
                {
                    var g = new GameObject("Digit").AddComponent<SpriteRenderer>();
                    g.transform.SetParent(root, false);
                    if (material != null) g.sharedMaterial = material;
                    g.sortingOrder = sortingOrder;
                    glyphs[d] = g;
                }
                root.gameObject.SetActive(false);
                pool[i] = new Number { root = root, glyphs = glyphs };
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Show(float damage, HitKind kind, Vector2 pos)
        {
            if (Instance != null) Instance.Spawn(damage, kind, pos);
        }

        void Spawn(float damage, HitKind kind, Vector2 pos)
        {
            var n = pool[next];
            next = (next + 1) % PoolSize; // putaran = yang tertua lebih dulu dipakai ulang
            string text = Mathf.Min(DamageNumberFormat.Value(damage), 999).ToString();
            var (color, scale) = DamageNumberFormat.Style(kind);
            for (int d = 0; d < MaxDigits; d++)
            {
                var g = n.glyphs[d];
                g.enabled = d < text.Length;
                if (!g.enabled) continue;
                g.sprite = digits[text[d] - '0'];
                g.color = color;
                g.transform.localPosition = new Vector3((d - (text.Length - 1) / 2f) * DigitWidth, 0f, 0f);
            }
            n.color = color;
            n.root.localScale = Vector3.one * scale;
            n.root.position = pos + Vector2.up * SpawnHeight;
            n.velocity = new Vector2(Random.Range(-Spread, Spread), Rise);
            n.born = Time.time;
            n.active = true;
            n.root.gameObject.SetActive(true);
        }

        void Update()
        {
            float dt = Time.deltaTime, now = Time.time;
            foreach (var n in pool)
            {
                if (!n.active) continue;
                float age = now - n.born;
                if (age >= Life)
                {
                    n.active = false;
                    n.root.gameObject.SetActive(false);
                    continue;
                }
                n.velocity.y -= Gravity * dt;
                n.root.position += (Vector3)(n.velocity * dt);
                float a = Mathf.Clamp01((Life - age) / FadeTime);
                foreach (var g in n.glyphs) g.color = new Color(n.color.r, n.color.g, n.color.b, a);
            }
        }
    }
}
