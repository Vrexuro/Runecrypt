using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using Runecrypt.Game3D;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests3D
{
    // Pola ProjectilePlayModeTests (memori unity-playmode-test-gotchas): EnterPlayMode di tingkat teratas,
    // langkah fisika sungguhan lewat Time.fixedTime, objek di x >= 100 supaya scene M1 tidak mengganggu.
    public class Combat3DPlayModeTests
    {
        class Dummy : MonoBehaviour, IDamageable
        {
            public int Hits;
            public void TakeHit(HitInfo hit) => Hits++;
        }

        static IEnumerator Steps(int n)
        {
            float target = Time.fixedTime + n * Time.fixedDeltaTime - 1e-4f;
            while (Time.fixedTime < target) yield return new WaitForFixedUpdate();
        }

        static Dummy MakeTarget(float x)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.layer = LayerMask.NameToLayer("Enemies");
            go.transform.position = new Vector3(x, 0.5f, 0f);
            return go.AddComponent<Dummy>();
        }

        static GameObject MakeWall(float x, float thickness = 0.4f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.layer = LayerMask.NameToLayer("Walls");
            go.transform.position = new Vector3(x, 0.5f, 0f);
            go.transform.localScale = new Vector3(thickness, 1.5f, 2f);
            return go;
        }

        static Fireball3D MakeFireball(float x, float speed = 12f)
        {
            var go = new GameObject("Fireball3D");
            go.transform.position = new Vector3(x, 0.5f, 0f);
            var f = go.AddComponent<Fireball3D>();
            f.Speed = speed;
            f.Launch(Vector2.right);
            return f;
        }

        [UnityTest]
        public IEnumerator Fireball3D_HitsEnemyInFront()
        {
            yield return new EnterPlayMode();
            var target = MakeTarget(103f);
            Physics.SyncTransforms();
            var f = MakeFireball(100f);
            yield return Steps(30);
            Assert.AreEqual(1, target.Hits);
            Assert.IsTrue(f == null, "bola api hilang setelah kena");
            Object.Destroy(target.gameObject);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Fireball3D_StopsAtWall()
        {
            yield return new EnterPlayMode();
            var wall = MakeWall(102f);
            var target = MakeTarget(104f);
            Physics.SyncTransforms();
            var f = MakeFireball(100f);
            yield return Steps(40);
            Assert.AreEqual(0, target.Hits);
            Assert.IsTrue(f == null);
            Object.Destroy(wall); Object.Destroy(target.gameObject);
            yield return new ExitPlayMode();
        }

        // Review Focus 3: bola api cepat tidak menembus dinding tipis di antara dua langkah fisika.
        [UnityTest]
        public IEnumerator Fireball3D_FastShotDoesNotTunnel()
        {
            yield return new EnterPlayMode();
            var wall = MakeWall(102f, 0.2f);
            var target = MakeTarget(104f);
            Physics.SyncTransforms();
            var f = MakeFireball(100f, 60f);
            yield return Steps(20);
            Assert.AreEqual(0, target.Hits);
            Assert.IsTrue(f == null);
            Object.Destroy(wall); Object.Destroy(target.gameObject);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Fireball3D_DiesAfterRange()
        {
            yield return new EnterPlayMode();
            var f = MakeFireball(100f);
            yield return Steps(30);               // 30 langkah × 0,02 × 12 = 7,2 > jangkauan 7
            Assert.IsTrue(f == null);
            yield return new ExitPlayMode();
        }

        // Review Focus 4: pemain tidak terlempar dari lantai.
        [UnityTest]
        public IEnumerator Player3D_StaysOnFloor()
        {
            yield return new EnterPlayMode();
            var go = new GameObject("P3D");
            go.transform.position = new Vector3(100f, 0f, 0f);
            go.AddComponent<CapsuleCollider>().center = new Vector3(0f, 0.5f, 0f);
            go.AddComponent<Rigidbody>();
            go.AddComponent<Player3D>();
            yield return Steps(2);
            go.GetComponent<Rigidbody>().AddForce(Vector3.up * 500f, ForceMode.VelocityChange);
            yield return Steps(10);
            Assert.AreEqual(0f, go.transform.position.y, 1e-3f);
            Object.Destroy(go);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Player3D_HitInvulnerability()
        {
            yield return new EnterPlayMode();
            var go = new GameObject("P3D");
            go.transform.position = new Vector3(100f, 0f, 0f);
            go.AddComponent<Rigidbody>();
            var p = go.AddComponent<Player3D>();
            yield return null;
            p.TakeHit(new HitInfo(1f, Element.None, false, Vector2.zero));
            p.TakeHit(new HitInfo(1f, Element.None, false, Vector2.zero));
            Assert.AreEqual(5f, p.Health.Current, "pukulan kedua dalam masa kebal diabaikan");
            Object.Destroy(go);
            yield return new ExitPlayMode();
        }
    }
}
