using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using Runecrypt.Game3D;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace Runecrypt.Tests3D
{
    // Temuan Important dari review akhir branch 3D (ledger: Final review).
    public class ReviewFixTests
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

        // Important 1: tembakan jarak dekat muncul di dalam collider musuh yang menempel; SphereCast tidak melihatnya.
        [UnityTest]
        public IEnumerator Fireball3D_SpawnedInsideEnemy_StillHits()
        {
            yield return new EnterPlayMode();
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.layer = LayerMask.NameToLayer("Enemies");
            target.transform.position = new Vector3(100.6f, 0.5f, 0f);    // menempel: kubus 100,1..101,1
            var dummy = target.AddComponent<Dummy>();
            Physics.SyncTransforms();
            var go = new GameObject("Fireball3D");
            go.transform.position = new Vector3(100.5f, 0.5f, 0f);       // muncul di dalam kubus
            go.AddComponent<Fireball3D>().Launch(Vector2.right);
            yield return Steps(5);
            Assert.AreEqual(1, dummy.Hits);
            Object.Destroy(target);
            yield return new ExitPlayMode();
        }

        // Important 2: titik cahaya pemain harus di pusat pemain, bukan 0,45 di depan (bisa masuk ke dalam pilar).
        [Test]
        public void Player3DPrefab_LightSourceAtPlayerCenter()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runecrypt3D/Prefabs/Player3D.prefab");
            var sources = prefab.GetComponentsInChildren<LightSource3D>(true);
            Assert.AreEqual(1, sources.Length);
            Assert.AreSame(prefab, sources[0].gameObject, "LightSource3D ada di root pemain");
        }

        // Important 3: bidik mouse dipotong di bidang setinggi bola api terbang, bukan di lantai.
        [Test]
        public void Player3D_AimPlaneAtMuzzleHeight()
        {
            var go = new GameObject("P3D", typeof(Rigidbody));
            var p = go.AddComponent<Player3D>();
            Assert.AreEqual(0.5f, p.AimPlaneHeight, 1e-4f);
            Object.DestroyImmediate(go);
        }

        // Important 4: renderer default URP tetap Renderer2D supaya scene 2D tidak rusak kalau branch ini di-merge.
        [Test]
        public void UrpDefaultRenderer_StaysRenderer2D()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
            Assert.AreEqual(0, new SerializedObject(urp).FindProperty("m_DefaultRendererIndex").intValue);
        }
    }
}
