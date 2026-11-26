using System.Collections;
using NUnit.Framework;
using Runecrypt.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // Bug: musuh mengejar dalam garis lurus, sehingga macet di balik pilar, nisan, atau dinding.
    // Berjalan di dalam scene M1 yang terbuka, jadi semua objek ditaruh di x ≥ 300 (jauh dari arena dan test lain).
    public class EnemyChasePlayModeTests
    {
        const float X = 300f;

        static GameObject Box(Vector2 center, Vector2 size, string layer = "Walls")
        {
            var go = new GameObject("TestObstacle") { layer = LayerMask.NameToLayer(layer) };
            go.transform.position = center;
            go.AddComponent<BoxCollider2D>().size = size;
            return go;
        }

        static float DistanceAfterChase(Vector2 player, Enemy e) =>
            Vector2.Distance(player, e.transform.position);

        [UnityTest]
        public IEnumerator Enemy_GoesAroundWallBetweenItAndPlayer()
        {
            yield return new EnterPlayMode();
            Vector2 player = new Vector2(X, 0.5f);
            CryptKitPlayModeTests.MakePlayer(player);
            Box(new Vector2(X + 3.5f, 0.5f), new Vector2(1f, 4f));
            var e = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 6.5f, 0.5f), 100f, speed: 3f);
            yield return ProjectilePlayModeTests.Steps(250);
            float d = DistanceAfterChase(player, e);
            yield return new ExitPlayMode();
            Assert.Less(d, 1.2f, "musuh harus memutari dinding dan sampai ke pemain");
        }

        // Ceruk ⊐ yang terbuka menjauhi pemain: musuh harus mundur dulu, baru memutar.
        [UnityTest]
        public IEnumerator Enemy_LeavesPocketOpeningAwayFromPlayer()
        {
            yield return new EnterPlayMode();
            Vector2 player = new Vector2(X, 20.5f);
            CryptKitPlayModeTests.MakePlayer(player);
            Box(new Vector2(X + 4.5f, 20.5f), new Vector2(1f, 5f));   // sisi kiri ceruk (menghadap pemain)
            Box(new Vector2(X + 7f, 22.5f), new Vector2(4f, 1f));     // atap
            Box(new Vector2(X + 7f, 18.5f), new Vector2(4f, 1f));     // lantai; terbuka ke kanan
            var e = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 6.5f, 20.5f), 100f, speed: 3f);
            yield return ProjectilePlayModeTests.Steps(450);
            float d = DistanceAfterChase(player, e);
            yield return new ExitPlayMode();
            Assert.Less(d, 1.2f, "musuh harus keluar dari ceruk lalu memutar ke pemain");
        }

        // Guci Abu dan Anglo ada di layer Props, bukan Walls; musuh juga harus memutarinya.
        [UnityTest]
        public IEnumerator Enemy_GoesAroundPropBetweenItAndPlayer()
        {
            yield return new EnterPlayMode();
            Vector2 player = new Vector2(X, 60.5f);
            CryptKitPlayModeTests.MakePlayer(player);
            Box(new Vector2(X + 3.5f, 60.5f), new Vector2(1f, 3f), "Props");
            var e = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 6.5f, 60.5f), 100f, speed: 3f);
            yield return ProjectilePlayModeTests.Steps(250);
            float d = DistanceAfterChase(player, e);
            yield return new ExitPlayMode();
            Assert.Less(d, 1.2f, "musuh harus memutari benda makam dan sampai ke pemain");
        }

        // Tanpa penghalang, musuh tetap mengejar lurus (perilaku lama tidak berubah).
        [UnityTest]
        public IEnumerator Enemy_ChasesStraightInOpenSpace()
        {
            yield return new EnterPlayMode();
            Vector2 player = new Vector2(X, 40.5f);
            CryptKitPlayModeTests.MakePlayer(player);
            var e = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 6.5f, 40.5f), 100f, speed: 3f);
            yield return ProjectilePlayModeTests.Steps(10);
            Vector2 v = e.GetComponent<Rigidbody2D>().linearVelocity;
            yield return new ExitPlayMode();
            Assert.Less(Mathf.Abs(v.y), 0.05f, "di ruang terbuka arah kejaran lurus ke pemain");
            Assert.Less(v.x, -1f);
        }
    }
}
