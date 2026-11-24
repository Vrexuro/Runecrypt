using System.Collections;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // M2.5 Isi Makam. Play mode di test runner memakai scene kosong, jadi setiap test membangun objeknya sendiri
    // di x ≥ 100.
    public class CryptKitPlayModeTests
    {
        const float X = 100f;

        static IEnumerator WaitUntilOrTimeout(System.Func<bool> done, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!done() && Time.realtimeSinceStartup < end) yield return null;
        }

        static PlayerController MakePlayer(Vector2 pos)
        {
            var go = new GameObject("TestPlayer");
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            return go.AddComponent<PlayerController>();
        }

        // Task 2: satu gelombang = satu ruangan; ArenaDirector memicu Started setiap gelombang baru.
        [UnityTest]
        public IEnumerator ArenaDirector_RaisesRoomStartedEachWave()
        {
            yield return new EnterPlayMode();
            MakePlayer(new Vector2(X, 0f));
            // Templat musuh diam jauh di luar arena uji; yang dibunuh hanya salinannya.
            var template = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 100f, 0f), hp: 5f);
            var spawn = new GameObject("Spawn").transform;
            spawn.position = new Vector2(X + 6f, 0f);
            var dirGo = new GameObject("Director");
            dirGo.SetActive(false);
            var director = dirGo.AddComponent<ArenaDirector>();
            var so = new SerializedObject(director);
            so.FindProperty("enemyPrefab").objectReferenceValue = template;
            var points = so.FindProperty("spawnPoints");
            points.arraySize = 1;
            points.GetArrayElementAtIndex(0).objectReferenceValue = spawn;
            so.FindProperty("waveDelay").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            int count = 0;
            RoomEvents.Started += () => count++;
            dirGo.SetActive(true);
            try
            {
                yield return WaitUntilOrTimeout(() => count >= 1, 5f);
                Assert.AreEqual(1, count, "gelombang 1");
                // WaitForSeconds tidak menunggu di test editor; tunggu sampai 3 musuh gelombang 1 selesai fade.
                yield return WaitUntilOrTimeout(() => Enemy.Active.Count >= 4, 5f);
                foreach (var e in Enemy.Active.Where(e => e != template).ToList())
                    e.TakeHit(new HitInfo(999f, Element.None, false, e.transform.position));
                yield return WaitUntilOrTimeout(() => count >= 2, 5f);
                Assert.AreEqual(2, count, "gelombang 2");
            }
            finally { RoomEvents.ClearForTests(); }
            yield return new ExitPlayMode();
        }

        // ---------- Task 4: Anglo ----------
        const string Prefabs = "Assets/Runecrypt/Prefabs/";

        static T Spawn<T>(string prefab, Vector2 pos) where T : Component =>
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab"), pos, Quaternion.identity)
                .GetComponent<T>();

        static HitInfo Hit(Element e, Vector2 at, float damage = 3f) => new HitInfo(damage, e, e == Element.Fire, at);

        [UnityTest]
        public IEnumerator Fireball_LightsBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X + 3f, 0f));
            yield return ProjectilePlayModeTests.Steps(2);
            ProjectilePlayModeTests.MakeFireball(new Vector2(X, 0f)).Launch(Vector2.right);
            yield return ProjectilePlayModeTests.Steps(30);
            bool lit = brazier.IsLit;
            yield return new ExitPlayMode();
            Assert.IsTrue(lit);
        }

        // Anglo yang menyala adalah sumber cahaya aturan dengan radius 2,5 (spec §5.2).
        [UnityTest]
        public IEnumerator LitBrazier_RevealsNearbyEnemy()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            Vector2 near = new Vector2(X + 2f, 0f), far = new Vector2(X + 3f, 0f);
            bool before = LightFieldHost.Field.IsLit(near);
            brazier.TakeHit(Hit(Element.Fire, brazier.transform.position));
            bool nearAfter = LightFieldHost.Field.IsLit(near), farAfter = LightFieldHost.Field.IsLit(far);
            yield return new ExitPlayMode();
            Assert.IsFalse(before, "padam: tidak menerangi");
            Assert.IsTrue(nearAfter, "2 tile: terang");
            Assert.IsFalse(farAfter, "3 tile: di luar radius 2,5");
        }

        [UnityTest]
        public IEnumerator IceHit_DoesNotLightBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            brazier.TakeHit(Hit(Element.Ice, brazier.transform.position));
            bool lit = brazier.IsLit;
            yield return new ExitPlayMode();
            Assert.IsFalse(lit);
        }

        [UnityTest]
        public IEnumerator RoomStarted_ExtinguishesBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            brazier.TakeHit(Hit(Element.Fire, brazier.transform.position));
            RoomEvents.RaiseStarted();
            bool lit = brazier.IsLit, lightOn = LightFieldHost.Field.IsLit(new Vector2(X + 1f, 0f));
            yield return new ExitPlayMode();
            Assert.IsFalse(lit);
            Assert.IsFalse(lightOn, "sumber cahaya ikut padam");
        }
    }
}
