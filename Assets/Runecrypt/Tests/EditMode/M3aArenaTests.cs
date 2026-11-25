using System.Linq;
using NUnit.Framework;
using Runecrypt.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runecrypt.Tests
{
    // M3a Task 8: prefab musuh baru, serpih es, Kristal Es, dan wiring arena.
    public class M3aArenaTests
    {
        const string Scene = "Assets/Runecrypt/Scenes/M1_Prototype.unity";
        const string Prefabs = "Assets/Runecrypt/Prefabs/";

        static GameObject Prefab(string name)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
            Assert.IsNotNull(go, name + " ada");
            return go;
        }

        static int TulangEyesOrder() => Prefab("TulangPenjaga").transform.Find("Eyes").GetComponent<SpriteRenderer>().sortingOrder;

        // Scene dibuka additive dan ditutup lagi (pola CryptKitTests); test tidak menyimpan scene.
        Scene opened, previousActive;
        bool closeOnTearDown;

        Scene OpenArena()
        {
            previousActive = SceneManager.GetActiveScene();
            opened = SceneManager.GetSceneByPath(Scene);
            closeOnTearDown = !opened.isLoaded;
            if (closeOnTearDown) opened = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(opened);
            return opened;
        }

        [TearDown]
        public void CloseArena()
        {
            if (!opened.IsValid()) return;
            if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
            if (closeOnTearDown) EditorSceneManager.CloseScene(opened, true);
            opened = default;
        }

        static T[] InScene<T>(Scene s) where T : Component =>
            s.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();

        [Test]
        public void Arena_HasThreeEnemyTypes()
        {
            var director = InScene<ArenaDirector>(OpenArena()).Single();
            var prefabs = new SerializedObject(director).FindProperty("enemyPrefabs");
            Assert.AreEqual(3, prefabs.arraySize);
            var defs = Enumerable.Range(0, 3).Select(i => ((Enemy)prefabs.GetArrayElementAtIndex(i).objectReferenceValue).Definition).ToList();
            Assert.AreEqual(3, defs.Distinct().Count(), "tiga jenis musuh berbeda");
        }

        [Test]
        public void Arena_CrystalsKeepSpawnsAndStartClear()
        {
            var scene = OpenArena();
            var crystals = InScene<ReactiveProp>(scene).Where(p => p.name.StartsWith("IceCrystal")).ToList();
            Assert.AreEqual(2, crystals.Count, "dua Kristal Es");
            var director = InScene<ArenaDirector>(scene).Single();
            var points = new SerializedObject(director).FindProperty("spawnPoints");
            var check = new System.Collections.Generic.List<Vector2> { InScene<PlayerController>(scene).Single().transform.position };
            for (int i = 0; i < points.arraySize; i++) check.Add(((Transform)points.GetArrayElementAtIndex(i).objectReferenceValue).position);
            var others = InScene<Collider2D>(scene).Where(c => c.gameObject.layer == LayerMask.NameToLayer("Props") || c.name == "PillarBody")
                .Where(c => c.GetComponentInParent<ReactiveProp>() == null).ToList();
            foreach (var crystal in crystals)
            {
                var b = crystal.GetComponent<Collider2D>().bounds;
                foreach (var p in check)
                    Assert.GreaterOrEqual(Vector2.Distance(b.ClosestPoint(p), p), 1.5f, $"{crystal.transform.position} terlalu dekat ke {p}");
                foreach (var o in others)
                    Assert.IsFalse(b.Intersects(o.bounds), $"{crystal.transform.position} menumpuk {o.name}");
            }
        }

        // Spec M2.6 §5.4: serpih musuh di atas efek pemain (<= 5) dan di bawah mata musuh; unlit supaya terbaca di gelap.
        [Test]
        public void EnemyShot_DrawsAbovePlayerEffectsBelowEyes()
        {
            var sr = Prefab("IceShard").GetComponent<SpriteRenderer>();
            StringAssert.Contains("Unlit", sr.sharedMaterial.name);
            Assert.AreEqual(9, sr.sortingOrder);
            Assert.Less(sr.sortingOrder, TulangEyesOrder());
            Assert.AreEqual(LayerMask.NameToLayer("EnemyShots"), Prefab("IceShard").layer);
        }

        [Test]
        public void NewEnemies_EyesOnTopAndWindupTelegraphWired()
        {
            int top = TulangEyesOrder();
            foreach (var name in new[] { "PelitaSesat", "BiarawanBeku" })
            {
                // RequireComponent pada otak menambah Enemy otomatis; builder tidak boleh menambah yang kedua.
                Assert.AreEqual(1, Prefab(name).GetComponents<Enemy>().Length, name + ": tepat satu Enemy");
                var eyes = Prefab(name).transform.Find("Eyes").GetComponent<SpriteRenderer>();
                Assert.AreEqual(top, eyes.sortingOrder, name);
                StringAssert.Contains("Unlit", eyes.sharedMaterial.name, name);
            }
            var swoop = new SerializedObject(Prefab("PelitaSesat").GetComponent<SwoopBrain>());
            Assert.IsNotNull(swoop.FindProperty("eyes").objectReferenceValue, "mata Pelita terpasang");
            Assert.IsNotNull(swoop.FindProperty("windupEyes").objectReferenceValue, "sprite mata membesar terpasang");
            Assert.IsNotNull(new SerializedObject(Prefab("BiarawanBeku").GetComponent<RangedBrain>()).FindProperty("shotPrefab").objectReferenceValue);
        }
    }
}
