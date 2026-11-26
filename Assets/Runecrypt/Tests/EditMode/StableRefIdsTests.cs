using System.IO;
using NUnit.Framework;
using Runecrypt.EditorTools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Runecrypt.Tests
{
    // Builder membangun ulang prefab dari GameObject baru setiap Build All. ShadowCaster2D menyimpan
    // provider bentuk bayangan sebagai [SerializeReference], yang id-nya (rid) acak kalau tidak distabilkan,
    // sehingga prefab tampil berubah di git walaupun desainnya sama.
    public class StableRefIdsTests
    {
        const string PrefabPath = "Assets/StableRefIdsTest_Temp.prefab";

        [TearDown]
        public void Cleanup() => AssetDatabase.DeleteAsset(PrefabPath);

        static string BuildCoverLikeBuilder()
        {
            var go = new GameObject("StableRefIdsTest");
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<BoxCollider2D>();
            go.AddComponent<ShadowCaster2D>();
            PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
            Object.DestroyImmediate(go);
            StableRefIds.RewriteFile(PrefabPath);
            return File.ReadAllText(PrefabPath);
        }

        [Test]
        public void PrefabShadowCaster_DibangunDuaKali_IsinyaSamaPersis()
        {
            var first = BuildCoverLikeBuilder();
            var second = BuildCoverLikeBuilder();

            Assert.That(first, Does.Contain("rid:"), "prefab harus menyimpan managed reference ShadowCaster2D");
            Assert.AreEqual(first, second);
        }

        [Test]
        public void PrefabShadowCaster_SetelahRidDitulisUlang_ProviderBayanganTetapTerbaca()
        {
            BuildCoverLikeBuilder();

            var caster = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath).GetComponent<ShadowCaster2D>();
            var ids = ManagedReferenceUtility.GetManagedReferenceIds(caster);

            Assert.That(ids, Has.Length.EqualTo(1));
            Assert.AreEqual(StableRefIds.StableId(PrefabPath + "#0"), ids[0]);
            Assert.IsNotNull(ManagedReferenceUtility.GetManagedReference(caster, ids[0]));
        }
    }
}
