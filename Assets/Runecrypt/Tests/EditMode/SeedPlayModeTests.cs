using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    public class RunTests
    {
        [TearDown] public void TearDown() => Run.ResetForTests();

        [Test]
        public void Begin_UsesGivenSeed()
        {
            Run.Begin(new RunSeed(5u));
            Assert.AreEqual(5u, Run.Seed.Value);
            Assert.AreEqual(5u, Run.Random.Seed.Value);
        }

        // Kelak dipakai input seed di menu: seed yang dipaksa hanya berlaku untuk run berikutnya.
        [Test]
        public void ForcedSeed_AppliesToNextRunOnly()
        {
            Run.ForceNextSeed(new RunSeed(9u));
            Run.BeginNext();
            Assert.AreEqual(9u, Run.Seed.Value);
            Run.BeginNext();
            Assert.AreNotEqual(9u, Run.Seed.Value);
        }

        [Test]
        public void Random_WithoutBegin_StartsARun()
        {
            Run.ResetForTests();
            Assert.IsNotNull(Run.Random);
            Assert.AreEqual(Run.Seed, Run.Random.Seed);
        }

        // Guci ke-n yang pecah memakai undian ke-n dari stream "loot" run ini.
        [Test]
        public void AshUrnDrop_DrawsFromRunLootStream()
        {
            var seed = new RunSeed(2024u);
            var expected = new RunRandom(seed).Stream("loot");
            Run.Begin(seed);
            var a = new GameObject("UrnA").AddComponent<AshUrn>();
            var b = new GameObject("UrnB").AddComponent<AshUrn>();
            try
            {
                Assert.AreEqual((float)expected.NextDouble(), b.random(), "guci mana pun, undian pertama sama");
                Assert.AreEqual((float)expected.NextDouble(), a.random());
            }
            finally
            {
                Object.DestroyImmediate(a.gameObject);
                Object.DestroyImmediate(b.gameObject);
            }
        }
    }

    // Bukti seed berfungsi, bukan pajangan: arena dijalankan ulang dengan seed yang sama harus menghasilkan urutan
    // titik spawn dan relic yang persis sama; seed lain menghasilkan urutan lain.
    public class SeedPlayModeTests
    {
        const float X = 500f;

        static (ArenaDirector director, Enemy template, RelicPickup pickupTemplate) MakeArena()
        {
            CryptKitPlayModeTests.MakePlayer(new Vector2(X, 0f));
            var template = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 200f, 0f), hp: 5f);

            var pickupGo = new GameObject("PickupTemplate");
            pickupGo.transform.position = new Vector2(X + 300f, 0f);
            var pickupTemplate = pickupGo.AddComponent<RelicPickup>();

            var dirGo = new GameObject("Director");
            dirGo.SetActive(false);
            var director = dirGo.AddComponent<ArenaDirector>();
            var so = new SerializedObject(director);
            so.FindProperty("enemyPrefab").objectReferenceValue = template;
            var points = so.FindProperty("spawnPoints");
            points.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                var p = new GameObject("Spawn" + i).transform;
                p.position = new Vector2(X + 6f + i, (i % 2 == 0 ? 3f : -3f));
                points.GetArrayElementAtIndex(i).objectReferenceValue = p;
            }
            var pool = so.FindProperty("relicPool");
            pool.arraySize = 6;
            for (int i = 0; i < 6; i++)
            {
                var r = ScriptableObject.CreateInstance<RelicDefinition>();
                r.name = r.displayName = "Relic" + i;
                pool.GetArrayElementAtIndex(i).objectReferenceValue = r;
            }
            so.FindProperty("relicPickupPrefab").objectReferenceValue = pickupTemplate;
            so.FindProperty("pickupPoint").vector2Value = new Vector2(X + 1f, 5f);
            so.FindProperty("waveDelay").floatValue = 0.2f;
            so.FindProperty("spawnFade").floatValue = 0.05f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return (director, template, pickupTemplate);
        }

        // Menjalankan satu ruangan (3 gelombang): mencatat titik spawn tiap batch (urut di dalam batch) dan relic
        // pertama yang ditawarkan.
        // Dijalankan di dalam satu sesi Play mode (EnterPlayMode hanya bekerja di level teratas UnityTest);
        // semua objek arena dibuang di akhir supaya run berikutnya mulai bersih.
        static IEnumerator RecordRoom(RunSeed seed, List<string> log)
        {
            var before = new HashSet<GameObject>(Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None));
            Run.ForceNextSeed(seed);
            var (director, template, pickupTemplate) = MakeArena();
            director.gameObject.SetActive(true);
            var seen = new HashSet<Enemy>();
            float end = Time.realtimeSinceStartup + 20f;
            string relic = null;
            while (relic == null && Time.realtimeSinceStartup < end)
            {
                var fresh = Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .Where(e => e != template && seen.Add(e))
                    .Select(e => $"{e.transform.position.x - X:0},{e.transform.position.y:0}")
                    .OrderBy(s => s, System.StringComparer.Ordinal).ToList();
                if (fresh.Count > 0) log.Add("spawn " + string.Join(" ", fresh));
                foreach (var e in Enemy.Active.Where(e => e != template && e.enabled).ToList())
                    e.TakeHit(new HitInfo(999f, Element.None, false, e.transform.position));
                var offered = Object.FindObjectsByType<RelicPickup>(FindObjectsSortMode.None)
                    .FirstOrDefault(p => p != pickupTemplate && p.Relic != null);
                if (offered != null) relic = offered.Relic.displayName;
                yield return null;
            }
            log.Add("relic " + relic);
            log.Add("seed " + Run.Seed);
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (!before.Contains(go) && go.transform.parent == null) Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SameSeed_SameSpawnsAndRelic_DifferentSeed_Differs()
        {
            var a1 = new List<string>();
            var a2 = new List<string>();
            var b = new List<string>();
            yield return new EnterPlayMode();
            yield return RecordRoom(new RunSeed(1111u), a1);
            yield return RecordRoom(new RunSeed(1111u), a2);
            yield return RecordRoom(new RunSeed(2222u), b);
            yield return new ExitPlayMode();
            TestContext.WriteLine("A: " + string.Join(" | ", a1));
            TestContext.WriteLine("B: " + string.Join(" | ", b));

            Assert.That(a1.Any(l => l.StartsWith("relic Relic")), "ruangan selesai dan relic ditawarkan");
            Assert.GreaterOrEqual(a1.Count(l => l.StartsWith("spawn")), 3, "tiga gelombang tercatat");
            CollectionAssert.AreEqual(a1, a2, "seed sama → isi run sama");
            CollectionAssert.AreNotEqual(a1, b, "seed lain → isi run lain");
        }
    }
}
