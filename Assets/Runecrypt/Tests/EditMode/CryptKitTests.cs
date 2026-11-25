using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Runecrypt.Tests
{
    // M2.5 Isi Makam (Docs/specs/2026-11-24-isi-makam-design.md).
    public class CryptKitTests
    {
        static int L(string name) => LayerMask.NameToLayer(name);

        [Test]
        public void PropsLayer_Exists() => Assert.GreaterOrEqual(L("Props"), 0);

        // Anglo dan Guci padat bagi pemain dan musuh, dan bisa dikenai tembakan; uap tidak menyentuhnya.
        [Test]
        public void PropsLayer_CollidesWithActorsNotReactives()
        {
            int props = L("Props");
            Assert.GreaterOrEqual(props, 0);
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Player")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Enemies")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Projectiles")));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(props, L("Reactives")));
        }

        // Task 1: logika inti.
        [Test]
        public void Brazier_IgnitesOnlyOnFire()
        {
            var b = new BrazierState();
            Assert.IsFalse(b.TryIgnite(Element.Ice));
            Assert.IsFalse(b.TryIgnite(Element.Lightning));
            Assert.IsFalse(b.TryIgnite(Element.None));
            Assert.IsFalse(b.IsLit);
            Assert.IsTrue(b.TryIgnite(Element.Fire));
            Assert.IsTrue(b.IsLit);
        }

        [Test]
        public void Brazier_IgnitesOnce()
        {
            var b = new BrazierState();
            b.TryIgnite(Element.Fire);
            Assert.IsFalse(b.TryIgnite(Element.Fire));
            Assert.IsTrue(b.IsLit);
        }

        [Test]
        public void Brazier_ResetExtinguishes()
        {
            var b = new BrazierState();
            b.TryIgnite(Element.Fire);
            b.Reset();
            Assert.IsFalse(b.IsLit);
        }

        [Test]
        public void Breakable_BreaksExactlyAtZero()
        {
            var s = new BreakableState(3f);
            Assert.IsFalse(s.Hit(2f));
            Assert.IsFalse(s.IsBroken);
            Assert.IsTrue(s.Hit(1f));
            Assert.IsTrue(s.IsBroken);
            Assert.IsFalse(s.Hit(5f));
        }

        [Test]
        public void Breakable_ResetRestoresHp()
        {
            var s = new BreakableState(3f);
            s.Hit(3f);
            s.Reset();
            Assert.IsFalse(s.IsBroken);
            Assert.AreEqual(3f, s.Hp);
        }

        // ---------- Task 7: arena yang ditata (scene M1 setelah Build All) ----------
        const string Scene = "Assets/Runecrypt/Scenes/M1_Prototype.unity";

        // Scene M1 dibuka additive lalu ditutup lagi di TearDown. Membukanya sebagai scene tunggal membuat test play
        // mode sesudahnya ikut memuat pemain M1.
        Scene opened, previousActive;
        bool closeOnTearDown;

        Transform Kit()
        {
            previousActive = SceneManager.GetActiveScene();
            opened = SceneManager.GetSceneByPath(Scene);
            closeOnTearDown = !opened.isLoaded;
            if (closeOnTearDown) opened = EditorSceneManager.OpenScene(Scene, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(opened);
            var root = opened.GetRootGameObjects().FirstOrDefault(g => g.name == "CryptKit");
            Assert.IsNotNull(root, "root CryptKit ada di scene");
            return root.transform;
        }

        [TearDown]
        public void CloseKitScene()
        {
            if (!opened.IsValid()) return;
            if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
            if (closeOnTearDown) EditorSceneManager.CloseScene(opened, true);
            opened = default;
        }

        static int CountNamed(Transform root, string prefix) =>
            root.GetComponentsInChildren<Transform>(true).Count(t => t.name.StartsWith(prefix) && t.parent != null && t.parent.name == "Props");

        [Test]
        public void CryptKit_HasExpectedProps()
        {
            var kit = Kit();
            // M2.6: arena 30x17, kit diskalakan (spec M2.6 §5.1).
            Assert.AreEqual(3, kit.GetComponentsInChildren<Brazier>(true).Length, "Anglo");
            Assert.AreEqual(5, kit.GetComponentsInChildren<AshUrn>(true).Length, "Guci");
            Assert.AreEqual(3, CountNamed(kit, "Tombstone"), "Nisan");
            Assert.AreEqual(2, CountNamed(kit, "Sarcophagus"), "Sarkofagus");
        }

        [Test]
        public void Arena_IsThirtyBySeventeenWithClosedWalls()
        {
            Kit();
            var walls = GameObject.Find("Grid/Walls").GetComponent<Tilemap>();
            int border = 0;
            for (int x = -15; x < 15; x++)
            for (int y = -9; y < 8; y++)
            {
                bool edge = x == -15 || x == 14 || y == -9 || y == 7;
                Assert.AreEqual(edge, walls.HasTile(new Vector3Int(x, y, 0)), $"sel {x},{y}");
                if (edge) border++;
            }
            Assert.AreEqual(90, border);
            Assert.AreEqual(4, Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Count(t => t.name == "PillarBody"), "4 pilar");
            var room = new SerializedObject(Object.FindFirstObjectByType<CameraFollow>()).FindProperty("room").rectValue;
            Assert.AreEqual(new Rect(-15f, -9f, 30f, 17f), room, "batas kamera = arena");
        }

        [Test]
        public void CryptKit_FloorUsesVariants()
        {
            Kit();
            var floor = GameObject.Find("Grid/Floor").GetComponent<Tilemap>();
            var counts = new Dictionary<string, int>();
            int total = 0;
            foreach (var p in floor.cellBounds.allPositionsWithin)
            {
                var t = floor.GetTile(p);
                if (t == null) continue;
                counts[t.name] = counts.TryGetValue(t.name, out int n) ? n + 1 : 1;
                total++;
            }
            Assert.GreaterOrEqual(counts.Count, 4, "beberapa variasi ubin dipakai");
            Assert.That(counts.TryGetValue("CryptFloor_plain", out int plain) ? plain / (float)total : 0f, Is.InRange(0.6f, 0.8f),
                "ubin polos dominan (~70%)");
        }

        // Review Focus 5: benda tidak menutupi titik spawn musuh maupun posisi awal pemain.
        [Test]
        public void CryptKit_KeepsSpawnsAndStartClear()
        {
            var kit = Kit();
            var director = Object.FindFirstObjectByType<ArenaDirector>();
            var points = new SerializedObject(director).FindProperty("spawnPoints");
            var check = new List<Vector2> { PlayerController.FindFirstObjectByType<PlayerController>().transform.position };
            for (int i = 0; i < points.arraySize; i++)
                check.Add(((Transform)points.GetArrayElementAtIndex(i).objectReferenceValue).position);
            foreach (var col in kit.GetComponentsInChildren<Collider2D>(true))
            foreach (var p in check)
            {
                var b = col.bounds;
                float d = Vector2.Distance(b.ClosestPoint(p), p);
                Assert.GreaterOrEqual(d, 1.5f, $"{col.name} terlalu dekat ke {p}");
            }
        }

        // Review akhir Important 1: menjalankan test tidak boleh menulis ulang scene yang di-commit.
        [Test]
        public void CryptKit_IsIdempotent()
        {
            Kit();
            Assume.That(closeOnTearDown, "scene M1 sedang terbuka di editor; test ini tidak mengubahnya");
            var before = System.IO.File.ReadAllBytes(Scene);
            EditorApplication.ExecuteMenuItem("Runecrypt/M2.5/Dress Arena");
            EditorApplication.ExecuteMenuItem("Runecrypt/M2.5/Dress Arena");
            CollectionAssert.AreEqual(before, System.IO.File.ReadAllBytes(Scene), "file scene di disk tidak berubah");
            var kits = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(t => t.name == "CryptKit" && t.parent == null).ToList();
            Assert.AreEqual(1, kits.Count, "satu root saja");
            Assert.AreEqual(5, kits[0].GetComponentsInChildren<AshUrn>(true).Length);
        }

        [Test]
        public void Loot_RollRespectsChance()
        {
            Assert.IsTrue(Loot.Roll(0.15f, 0.149f));
            Assert.IsFalse(Loot.Roll(0.15f, 0.15f));
            Assert.IsFalse(Loot.Roll(0f, 0f));
        }
    }
}
