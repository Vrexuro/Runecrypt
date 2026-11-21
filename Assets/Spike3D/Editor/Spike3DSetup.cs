using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Spike3D
{
    // SPIKE 3D (boleh dibuang bersama folder Assets/Spike3D): scene uji Tulang Penjaga 3D.
    // Game 2D tidak disentuh. Kamera scene ini memakai renderer kedua (Universal) di aset URP yang sama,
    // karena Renderer2D tidak menerangi mesh 3D.
    public static class Spike3DSetup
    {
        const string Dir = "Assets/Spike3D/";
        const string ModelPath = Dir + "Models/TulangPenjaga3D.fbx";
        const string ScenePath = Dir + "Spike3D_Tulang.unity";

        [MenuItem("Runecrypt/Spike3D/Build Scene")]
        public static void Build()
        {
            int rendererIndex = EnsureUniversalRenderer();
            RemapMaterials();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.07f, 0.07f, 0.1f);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            floor.GetComponent<Renderer>().sharedMaterial = Lit("Spike_Floor", new Color(0.09f, 0.08f, 0.1f));

            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));
            model.name = "TulangPenjaga3D";
            model.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // ekspor FBX dari Blender membalik arah depan

            // pembanding: sprite 2D sekarang di sebelahnya (unlit, karena cahaya 2D tidak ada di renderer ini)
            var sprite = new GameObject("Tulang2D").AddComponent<SpriteRenderer>();
            sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Runecrypt/Art/Prototype/skeleton_0.png");
            sprite.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            sprite.transform.position = new Vector3(1.1f, 0.5f, 0f);
            sprite.transform.rotation = Quaternion.Euler(40f, 0f, 0f);

            var player = new GameObject("PlayerLight").AddComponent<Light>();
            player.type = LightType.Point;
            player.color = new Color(1f, 0.55f, 0.25f);
            player.range = 4f;
            player.intensity = 3f;
            player.shadows = LightShadows.Soft;
            player.transform.position = new Vector3(-0.7f, 0.8f, -0.9f);

            var room = new GameObject("RoomLight").AddComponent<Light>();
            room.type = LightType.Directional;
            room.color = new Color(0.6f, 0.6f, 0.9f);
            room.intensity = 0.25f;
            room.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

            var cam = new GameObject("SpikeCamera").AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.025f);
            cam.transform.position = new Vector3(0.5f, 2.6f, -2.2f);
            cam.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
            cam.GetUniversalAdditionalCameraData().SetRenderer(rendererIndex);
            cam.tag = "MainCamera";

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Spike3D: scene {ScenePath} dibuat, renderer index {rendererIndex}");
        }

        static int EnsureUniversalRenderer()
        {
            // Pipeline proyek diatur lewat Quality Settings, jadi GraphicsSettings.defaultRenderPipeline bisa null.
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/UniversalRP.asset");
            const string path = Dir + "Spike3DRenderer.asset";
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(data, path);
            }
            var so = new SerializedObject(urp);
            var list = so.FindProperty("m_RendererDataList");
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == data) return i;
            list.arraySize++;
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return list.arraySize - 1;
        }

        static void RemapMaterials()
        {
            var mats = new Dictionary<string, Material>
            {
                { "Tulang_Bone", Lit("Tulang_Bone", Hex(222, 214, 192)) },
                { "Tulang_BoneDark", Lit("Tulang_BoneDark", Hex(165, 156, 136)) },
                { "Tulang_Ink", Lit("Tulang_Ink", Hex(20, 15, 24)) },
                { "Tulang_Eye", Lit("Tulang_Eye", Hex(255, 30, 20), new Color(4f, 0.4f, 0.25f)) },
                { "Tulang_Rust", Lit("Tulang_Rust", Hex(138, 72, 40), metal: 0.2f) },
                { "Tulang_RustDark", Lit("Tulang_RustDark", Hex(92, 48, 28), metal: 0.4f) },
                { "Tulang_Iron", Lit("Tulang_Iron", Hex(72, 68, 78), metal: 0.8f, smooth: 0.5f) },
                { "Tulang_Wood", Lit("Tulang_Wood", Hex(96, 58, 34)) },
            };
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            foreach (var kv in mats) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key), kv.Value);
            importer.SaveAndReimport();
        }

        static Color Hex(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);

        static Material Lit(string name, Color baseColor, Color? emission = null, float metal = 0f, float smooth = 0.2f)
        {
            string path = Dir + "Materials/" + name + ".mat";
            System.IO.Directory.CreateDirectory(Dir + "Materials");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Metallic", metal);
            mat.SetFloat("_Smoothness", smooth);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;   // None membuat URP 17 mematikan _EMISSION
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
