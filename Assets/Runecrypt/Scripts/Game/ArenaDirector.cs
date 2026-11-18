using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Runecrypt.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runecrypt.Game
{
    // Loop arena prototipe M1: gelombang musuh tanpa akhir, layar tumbang, dan ulang dengan R.
    public class ArenaDirector : MonoBehaviour
    {
        [SerializeField] Enemy enemyPrefab;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] float minSpawnDistance = 4f;
        [SerializeField] float waveDelay = 3f;
        [SerializeField] float spawnFade = 0.5f;
        [SerializeField] int maxPerWave = 6;

        // shortcut: pickup relic setiap gelombang hanya untuk mencoba relic di M2, diganti ruang harta di M4.
        [Header("Relic (M2)")]
        [SerializeField] List<RelicDefinition> relicPool = new List<RelicDefinition>();
        [SerializeField] RelicPickup relicPickupPrefab;
        [SerializeField] Vector2 pickupPoint;

        readonly System.Random rng = new System.Random();
        RelicPickup currentPickup;

        RunecryptControls controls;
        PlayerController player;
        int wave, alive;
        float nextWaveAt, fps;
        bool playerDead;

        void Awake()
        {
            GamePause.IsPaused = false; // state statis dari run sebelumnya (ulang dengan R)
            RemainsRegistry.Clear();
            Time.timeScale = 1f;
            controls = new RunecryptControls();
        }

        void OnEnable()
        {
            Enemy.AnyDied += OnEnemyDied;
            controls.Enable();
        }

        void OnDisable()
        {
            Enemy.AnyDied -= OnEnemyDied;
            controls.Disable();
        }

        void OnDestroy() => controls.Dispose();

        void Start()
        {
            player = PlayerController.Instance;
            player.Died += OnPlayerDied;
            nextWaveAt = Time.time + 1f;
        }

        void Update()
        {
            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.1f);

            if (playerDead)
            {
                if (controls.Gameplay.Restart.WasPressedThisFrame())
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (alive == 0 && Time.time >= nextWaveAt) SpawnWave();
        }

        void SpawnWave()
        {
            wave++;
            int count = Mathf.Min(2 + wave, maxPerWave);
            Vector2 p = player.transform.position;
            var far = spawnPoints.Where(s => Vector2.Distance(s.position, p) >= minSpawnDistance).ToList();
            if (far.Count == 0) far = spawnPoints.ToList();
            Shuffle(far);
            alive += count;
            for (int i = 0; i < count; i++) StartCoroutine(SpawnAfterFade(far[i % far.Count].position));
        }

        IEnumerator SpawnAfterFade(Vector3 pos)
        {
            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            var body = enemy.GetComponent<Rigidbody2D>();
            var renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
            enemy.enabled = false;
            body.simulated = false;
            // shortcut: fade alfa sebagai pengganti efek asap gelap 0,5 detik; asap partikel di M2
            for (float t = 0f; t < spawnFade; t += Time.deltaTime)
            {
                foreach (var r in renderers) r.color = new Color(1f, 1f, 1f, t / spawnFade);
                yield return null;
            }
            foreach (var r in renderers) r.color = Color.white;
            enemy.enabled = true;
            body.simulated = true;
        }

        void OnEnemyDied(Enemy e)
        {
            alive = Mathf.Max(0, alive - 1);
            if (alive == 0)
            {
                nextWaveAt = Time.time + waveDelay;
                SpawnRelicPickup();
            }
        }

        void SpawnRelicPickup()
        {
            if (relicPickupPrefab == null || player == null || currentPickup != null) return;
            var relic = RelicPicker.PickUnowned(relicPool, player.Relics, rng);
            if (relic == null) return; // semua relic sudah dimiliki
            currentPickup = Instantiate(relicPickupPrefab, pickupPoint, Quaternion.identity);
            currentPickup.Init(relic);
        }

        void OnPlayerDied()
        {
            playerDead = true;
            GamePause.IsPaused = true; // supaya hit-stop yang sedang berjalan tidak mengembalikan timeScale ke 1
            Time.timeScale = 0f;
        }

        static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        void OnGUI()
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = 22 };
            style.normal.textColor = Color.white;
            float hp = player != null ? player.Health.Current : 0f;
            float maxHp = player != null ? player.Health.Max : 6f;
            GUI.Label(new Rect(16, 12, 600, 30), $"HP {hp:0.#}/{maxHp:0.#}   Gelombang {wave}   FPS {fps:0}", style);
            if (player != null && player.Relics.Count > 0)
                GUI.Label(new Rect(16, 40, 1200, 30), "Relic: " + string.Join(", ", player.Relics.Select(r => r.displayName)), style);

            if (!playerDead) return;
            var big = new GUIStyle(style) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
            big.normal.textColor = new Color(1f, 0.8f, 0.4f);
            GUI.Label(new Rect(0, Screen.height / 2f - 40, Screen.width, 80), "Tumbang. Tekan R untuk ulang", big);
        }
    }
}
