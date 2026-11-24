using System.IO;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.EditorTools
{
    // M2.5 Isi Makam (Docs/plans/2026-11-24-m2-5-isi-makam.md): prefab benda makam dan penataan arena.
    // Dipanggil dari M1ActorBuilder supaya "Runecrypt/M1/Build All" tetap mereproduksi scene.
    public static class M25CryptBuilder
    {
        const string CryptArt = "Assets/Runecrypt/Art/Crypt/";
        const string PrefabDir = "Assets/Runecrypt/Prefabs/";
        const string FeedbackDir = "Assets/Runecrypt/Data/Feedback/";
        const string UnlitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const float BrazierLightRadius = 2.5f; // spec §5.2

        public class Assets
        {
            public Brazier brazier;
        }

        [MenuItem("Runecrypt/M2.5/Build Crypt Assets")]
        public static Assets BuildAssets()
        {
            Directory.CreateDirectory(PrefabDir);
            var a = new Assets { brazier = BuildBrazier() };
            AssetDatabase.SaveAssets();
            return a;
        }

        public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(CryptArt + name + ".png");

        static Brazier BuildBrazier()
        {
            var go = new GameObject("Brazier") { layer = LayerMask.NameToLayer("Props") };
            var body = go.AddComponent<SpriteRenderer>();
            body.sprite = Sprite("brazier_off");
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.8f);

            var fire = new GameObject("Fire");
            fire.transform.SetParent(go.transform, false);
            var sr = fire.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite("brazier_on_0");
            sr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitMat); // api memancarkan cahaya
            var fb = fire.AddComponent<SpriteFlipbook>();
            var fso = new SerializedObject(fb);
            var frames = fso.FindProperty("frames");
            frames.arraySize = 4;
            for (int i = 0; i < 4; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = Sprite($"brazier_on_{i}");
            fso.FindProperty("fps").floatValue = 8f;
            fso.ApplyModifiedPropertiesWithoutUndo();
            var light = M1Builder.MakePointLight("Light", fire.transform, "#FF9A4A", BrazierLightRadius);
            light.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            var pulse = light.gameObject.AddComponent<LightPulse>();
            fire.SetActive(false);

            var profile = M1ActorBuilder.LoadOrCreate<FeedbackProfile>(FeedbackDir + "BrazierIgnite.asset", p =>
            {
                p.shakeTrauma = 0.1f;
                p.volume = 0.8f;
            });
            profile.particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "EmberBurst.prefab");
            EditorUtility.SetDirty(profile);

            var b = go.AddComponent<Brazier>();
            var so = new SerializedObject(b);
            so.FindProperty("unlitBody").objectReferenceValue = body;
            so.FindProperty("fire").objectReferenceValue = fire;
            so.FindProperty("pulse").objectReferenceValue = pulse;
            so.FindProperty("igniteProfile").objectReferenceValue = profile;
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go).GetComponent<Brazier>();
        }

        static GameObject Save(GameObject go)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + go.name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
