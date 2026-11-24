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
            public AshUrn urn;
            public HealingEmber ember;
            public GameObject tombstone;
            public GameObject sarcophagus;
            public Sprite pillarSprite;
        }

        [MenuItem("Runecrypt/M2.5/Build Crypt Assets")]
        public static Assets BuildAssets()
        {
            Directory.CreateDirectory(PrefabDir);
            var ember = BuildEmber();
            var a = new Assets
            {
                brazier = BuildBrazier(), ember = ember, urn = BuildUrn(ember),
                tombstone = BuildCover("Tombstone", "tombstone", new Vector2(0.75f, 0.8f), new Vector2(0f, -0.05f)),
                sarcophagus = BuildCover("Sarcophagus", "sarcophagus", new Vector2(1.85f, 0.8f), new Vector2(0f, -0.05f)),
                pillarSprite = Sprite("pillar"),
            };
            AssetDatabase.SaveAssets();
            return a;
        }

        static FeedbackProfile Profile(string name, float shake, float volume)
        {
            var p = M1ActorBuilder.LoadOrCreate<FeedbackProfile>(FeedbackDir + name + ".asset", x =>
            {
                x.shakeTrauma = shake;
                x.volume = volume;
            });
            return p;
        }

        static void SetFrames(SpriteFlipbook fb, string prefix, int count, float fps)
        {
            var so = new SerializedObject(fb);
            var frames = so.FindProperty("frames");
            frames.arraySize = count;
            for (int i = 0; i < count; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = Sprite($"{prefix}{i}");
            so.FindProperty("fps").floatValue = fps;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static HealingEmber BuildEmber()
        {
            var go = new GameObject("HealingEmber");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite("ember_pickup_0");
            sr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitMat); // berpendar tanpa memancarkan cahaya
            sr.sortingOrder = 1;
            SetFrames(go.AddComponent<SpriteFlipbook>(), "ember_pickup_", 2, 3f);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
            var e = go.AddComponent<HealingEmber>();
            var so = new SerializedObject(e);
            so.FindProperty("pickupProfile").objectReferenceValue = Profile("EmberPickup", 0f, 0.8f);
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go).GetComponent<HealingEmber>();
        }

        static AshUrn BuildUrn(HealingEmber ember)
        {
            var go = new GameObject("AshUrn") { layer = LayerMask.NameToLayer("Props") };
            var body = go.AddComponent<SpriteRenderer>();
            body.sprite = Sprite("urn_a");
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.6f, 0.7f);
            box.offset = new Vector2(0f, -0.1f);
            var flash = M1JuiceBuilder.AddHitFlash(go);
            var breakProfile = Profile("UrnBreak", 0.05f, 0.8f);
            breakProfile.particlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + "EmberBurst.prefab");
            EditorUtility.SetDirty(breakProfile);
            var urn = go.AddComponent<AshUrn>();
            var so = new SerializedObject(urn);
            so.FindProperty("emberPrefab").objectReferenceValue = ember;
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("wholeSprite").objectReferenceValue = Sprite("urn_a");
            so.FindProperty("brokenSprite").objectReferenceValue = Sprite("urn_broken");
            so.FindProperty("solid").objectReferenceValue = box;
            so.FindProperty("hitFlash").objectReferenceValue = flash;
            so.FindProperty("breakProfile").objectReferenceValue = breakProfile;
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go).GetComponent<AshUrn>();
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

        // Tempat berlindung (spec §5.4): seperti dinding — menahan gerak dan tembakan, dan memotong cahaya di layar
        // (ShadowCaster2D) maupun di aturan (raycast LightFieldHost membaca layer Walls).
        static GameObject BuildCover(string name, string sprite, Vector2 size, Vector2 offset)
        {
            var go = new GameObject(name) { layer = LayerMask.NameToLayer("Walls") };
            go.AddComponent<SpriteRenderer>().sprite = Sprite(sprite);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = size;
            box.offset = offset;
            go.AddComponent<UnityEngine.Rendering.Universal.ShadowCaster2D>();
            return Save(go);
        }

        static GameObject Save(GameObject go)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + go.name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
