using System.Collections.Generic;
using System.Linq;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runecrypt.Game3D
{
    // Loop arena 3D, mengikuti ArenaDirector 2D: gelombang tanpa akhir, layar tumbang, ulang dengan R.
    public class Arena3DDirector : MonoBehaviour
    {
        [SerializeField] Enemy3D enemyPrefab;
        [SerializeField] Transform[] spawnPoints;
        [SerializeField] float minSpawnDistance = 4f;
        [SerializeField] float waveDelay = 3f;
        [SerializeField] int maxPerWave = 6;

        RunecryptControls controls;
        Player3D player;
        int wave, alive;
        float nextWaveAt, fps;
        bool playerDead;

        void Awake()
        {
            GamePause.IsPaused = false;
            Time.timeScale = 1f;
            controls = new RunecryptControls();
        }

        void OnEnable() { Enemy3D.AnyDied += OnEnemyDied; controls.Enable(); }
        void OnDisable() { Enemy3D.AnyDied -= OnEnemyDied; controls.Disable(); }
        void OnDestroy() => controls.Dispose();

        void Start()
        {
            player = Player3D.Instance;
            player.Died += OnPlayerDied;
            nextWaveAt = Time.time + 1f;
        }

        void Update()
        {
            fps = Mathf.Lerp(fps, 1f / Mathf.Max(Time.unscaledDeltaTime, 1e-4f), 0.1f);
            if (playerDead)
            {
                if (controls.Gameplay.Restart.WasPressedThisFrame()) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            if (alive == 0 && Time.time >= nextWaveAt) SpawnWave();
        }

        void SpawnWave()
        {
            wave++;
            int count = Mathf.Min(2 + wave, maxPerWave);
            Vector3 p = player.transform.position;
            var far = spawnPoints.Where(s => Vector3.Distance(s.position, p) >= minSpawnDistance).ToList();
            if (far.Count == 0) far = spawnPoints.ToList();
            Shuffle(far);
            alive += count;
            for (int i = 0; i < count; i++) Instantiate(enemyPrefab, far[i % far.Count].position, Quaternion.identity);
        }

        void OnEnemyDied(Enemy3D e)
        {
            alive = Mathf.Max(0, alive - 1);
            if (alive == 0) nextWaveAt = Time.time + waveDelay;
        }

        void OnPlayerDied()
        {
            playerDead = true;
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
            GUI.Label(new Rect(16, 12, 700, 30), $"HP {hp:0.#}/6   Gelombang {wave}   FPS {fps:0}   (3D)", style);
            if (!playerDead) return;
            var big = new GUIStyle(style) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
            big.normal.textColor = new Color(1f, 0.8f, 0.4f);
            GUI.Label(new Rect(0, Screen.height / 2f - 40, Screen.width, 80), "Tumbang. Tekan R untuk ulang", big);
        }
    }
}
