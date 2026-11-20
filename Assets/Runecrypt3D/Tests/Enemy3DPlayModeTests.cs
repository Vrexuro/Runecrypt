using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game3D;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests3D
{
    // Tulang Penjaga 3D dengan aturan cahaya GDD §5.5 dari Runecrypt.Core.
    public class Enemy3DPlayModeTests
    {
        static IEnumerator Steps(int n)
        {
            float target = Time.fixedTime + n * Time.fixedDeltaTime - 1e-4f;
            while (Time.fixedTime < target) yield return new WaitForFixedUpdate();
        }

        static Enemy3D MakeEnemy(float x)
        {
            var go = new GameObject("Enemy3D");
            go.layer = LayerMask.NameToLayer("Enemies");
            go.transform.position = new Vector3(x, 0f, 0f);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.5f, 0f); col.radius = 0.3f; col.height = 1f;
            go.AddComponent<Rigidbody>();
            var e = go.AddComponent<Enemy3D>();
            e.Init(AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Runecrypt/Data/Enemies/TulangPenjaga.asset"),
                   AssetDatabase.LoadAssetAtPath<LightRules>("Assets/Runecrypt/Data/LightRules.asset"));
            return e;
        }

        static Player3D MakePlayer(float x)
        {
            var go = new GameObject("Player3D");
            go.layer = LayerMask.NameToLayer("Player");
            go.transform.position = new Vector3(x, 0f, 0f);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.5f, 0f); col.radius = 0.3f; col.height = 1f;
            go.AddComponent<Rigidbody>();
            return go.AddComponent<Player3D>();
        }

        static GameObject MakeLight(float x, float radius)
        {
            var go = new GameObject("Light3D");
            go.transform.position = new Vector3(x, 0.5f, 0f);
            go.AddComponent<LightSource3D>().Radius = radius;
            return go;
        }

        [UnityTest]
        public IEnumerator Enemy3D_LitTakes25PercentMore()
        {
            yield return new EnterPlayMode();
            var lit = MakeEnemy(100f);
            var dark = MakeEnemy(130f);
            var light = MakeLight(100f, 3f);
            yield return null;
            lit.TakeHit(new HitInfo(3f, Element.Fire, false, new Vector2(100f, 0f)));
            dark.TakeHit(new HitInfo(3f, Element.Fire, false, new Vector2(130f, 0f)));
            Assert.AreEqual(12f - 3.75f, lit.CurrentHp, 1e-4f);
            Assert.AreEqual(12f - 3f, dark.CurrentHp, 1e-4f);
            Object.Destroy(lit.gameObject); Object.Destroy(dark.gameObject); Object.Destroy(light);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Enemy3D_ChasesPlayer()
        {
            yield return new EnterPlayMode();
            var player = MakePlayer(110f);
            var e = MakeEnemy(100f);
            yield return Steps(50);
            Assert.Less(Vector3.Distance(e.transform.position, player.transform.position), 9f);
            Assert.AreEqual(0f, e.transform.position.y, 1e-3f, "musuh tetap di lantai");
            Object.Destroy(player.gameObject); Object.Destroy(e.gameObject);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Enemy3D_DarkIsFaster()
        {
            yield return new EnterPlayMode();
            var player = MakePlayer(150f);
            var lit = MakeEnemy(100f);
            var dark = MakeEnemy(200f);
            var light = MakeLight(100f, 20f);          // menerangi 80..120 saja
            yield return Steps(2);
            float litStart = lit.transform.position.x, darkStart = dark.transform.position.x;
            yield return Steps(20);
            float litMoved = lit.transform.position.x - litStart;
            float darkMoved = darkStart - dark.transform.position.x;
            Assert.Greater(litMoved, 0.1f);
            Assert.AreEqual(1.2f, darkMoved / litMoved, 0.05f);
            Object.Destroy(player.gameObject); Object.Destroy(lit.gameObject); Object.Destroy(dark.gameObject); Object.Destroy(light);
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Enemy3D_ContactDamagesPlayerOncePerInterval()
        {
            yield return new EnterPlayMode();
            var player = MakePlayer(100f);
            var e = MakeEnemy(100.7f);
            // 0,9 detik: lebih lama dari i-frame pemain (0,8) tapi kurang dari jeda sentuh Tulang (1 detik)
            yield return Steps(45);
            Assert.AreEqual(5f, player.Health.Current);
            Object.Destroy(player.gameObject); Object.Destroy(e.gameObject);
            yield return new ExitPlayMode();
        }
    }
}
