using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.Tests3D
{
    // Temuan playtest Arena3D 2026-11-21: karakter menghadap terbalik, mata Tulang tidak menyala.
    public class PlaytestFixTests
    {
        const string Prefabs = "Assets/Runecrypt3D/Prefabs/";

        // Rata-rata posisi dunia semua vertex yang memakai material mata.
        static Vector3 EyeCenter(GameObject go, string eyeMaterial)
        {
            Vector3 sum = Vector3.zero; int n = 0;
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            {
                var mesh = r.GetComponent<MeshFilter>().sharedMesh;
                var mats = r.sharedMaterials;
                var verts = mesh.vertices;
                for (int s = 0; s < mesh.subMeshCount && s < mats.Length; s++)
                {
                    if (mats[s] == null || !mats[s].name.StartsWith(eyeMaterial)) continue;
                    foreach (int i in mesh.GetIndices(s)) { sum += r.transform.TransformPoint(verts[i]); n++; }
                }
            }
            Assert.Greater(n, 0, "tidak ada vertex bermaterial " + eyeMaterial);
            return sum / n;
        }

        // Tanpa putaran di Visual, karakter membidik +Z; matanya harus ada di sisi +Z.
        [TestCase("Player3D", "Pyro_Eye")]
        [TestCase("Tulang3D", "Tulang_Eye")]
        public void Prefab_EyesFaceForward(string prefab, string eyeMaterial)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab"));
            try { Assert.Greater(EyeCenter(go, eyeMaterial).z - go.transform.position.z, 0.05f); }
            finally { Object.DestroyImmediate(go); }
        }

        // URP 17 (BaseShaderGUI.SetMaterialKeywords) mematikan _EMISSION saat validasi kalau flag GI tidak emissive.
        [TestCase("Assets/Spike3D/Materials/Tulang_Eye.mat")]
        [TestCase("Assets/Runecrypt3D/Materials/Pyro_Eye.mat")]
        [TestCase("Assets/Runecrypt3D/Materials/Pyro_Ember.mat")]
        [TestCase("Assets/Runecrypt3D/Materials/Pyro_Burn.mat")]
        public void GlowMaterial_KeepsEmissiveGiFlag(string path)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            Assert.AreNotEqual(MaterialGlobalIlluminationFlags.None, mat.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive);
        }
    }
}
