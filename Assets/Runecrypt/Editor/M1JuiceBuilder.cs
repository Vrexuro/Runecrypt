using System.IO;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.EditorTools
{
    // M1.5 (Docs/plans/2026-11-17-m1-5-juice.md Task 7): profil umpan balik, prefab efek, dan layanan scene.
    // Dipanggil dari M1ActorBuilder supaya "Runecrypt/M1/Build All" tetap mereproduksi scene.
    public static class M1JuiceBuilder
    {
        const string JuiceArt = "Assets/Runecrypt/Art/Juice/";
        const string AudioDir = "Assets/Runecrypt/Audio/";
        const string FeedbackDir = "Assets/Runecrypt/Data/Feedback/";
        const string PrefabDir = "Assets/Runecrypt/Prefabs/";
        const string MaterialDir = "Assets/Runecrypt/Materials/";
        const string UnlitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const string LitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";

        public class Assets
        {
            public FeedbackProfile shot, hitLit, hitDark, deathTulang, steam, playerHurt;
            public GameObject deathTulangFx, ashPile;
        }

        public static Assets BuildAssets()
        {
            Directory.CreateDirectory(FeedbackDir);
            Directory.CreateDirectory(MaterialDir);
            var emberBurst = BuildEmberBurst();
            var steamRing = BuildSteamRing();
            var a = new Assets
            {
                deathTulangFx = BuildDeathTulang(),
                ashPile = BuildAshPile(),
            };

            // Angka dari spec §5. LoadOrCreate: setelan manual di Inspector tidak tertimpa saat build ulang.
            a.shot = Profile("Shot", p => { p.clips = new[] { Clip("sfx_shot_6"), Clip("sfx_shot_7"), Clip("sfx_shot_9") }; });
            a.hitLit = Profile("HitLit", p =>
            {
                p.flashAlpha = 1f; p.flashDuration = 0.08f; p.hitStop = 0.03f;
                p.particlePrefab = emberBurst; p.clips = new[] { Clip("sfx_hit") };
            });
            a.hitDark = Profile("HitDark", p =>
            {
                p.flashAlpha = 0.35f; p.flashDuration = 0.08f; p.hitStop = 0.03f;
                p.clips = new[] { Clip("sfx_hit") }; p.volume = 0.63f; // −4 dB
            });
            a.deathTulang = Profile("DeathTulang", p =>
            {
                p.flashAlpha = 1f; p.flashDuration = 0.06f; p.hitStop = 0.06f; p.shakeTrauma = 0.25f;
                p.clips = new[] { Clip("sfx_death_tulang") };
            });
            a.steam = Profile("Steam", p =>
            {
                p.hitStop = 0.04f; p.shakeTrauma = 0.3f; p.particlePrefab = steamRing;
                p.clips = new[] { Clip("sfx_steam") };
            });
            a.playerHurt = Profile("PlayerHurt", p =>
            {
                p.flashColor = M1Builder.Hex("#E0474C"); p.flashAlpha = 1f; p.flashDuration = 0.1f;
                p.hitStop = 0.08f; p.shakeTrauma = 0.45f; // belum ada SFX (spec §8)
            });
            AssetDatabase.SaveAssets();
            return a;
        }

        // Child "Flash" berisi overlay kedipan yang menyalin sprite induk.
        public static HitFlash AddHitFlash(GameObject go)
        {
            var child = new GameObject("Flash");
            child.transform.SetParent(go.transform, false);
            var sr = child.AddComponent<SpriteRenderer>();
            sr.sharedMaterial = FlashMaterial();
            var flash = child.AddComponent<HitFlash>();
            flash.SetSource(go.GetComponent<SpriteRenderer>());
            return flash;
        }

        // Objek "Feedback" di scene dan CameraShake di kamera utama.
        public static void BuildSceneServices()
        {
            var go = new GameObject("Feedback");
            go.AddComponent<HitStop>();
            go.AddComponent<SfxPlayer>();
            var numbers = go.AddComponent<DamageNumbers>();
            var so = new SerializedObject(numbers);
            var digits = so.FindProperty("digits");
            digits.arraySize = 10;
            for (int i = 0; i < 10; i++) digits.GetArrayElementAtIndex(i).objectReferenceValue = JuiceSprite($"digit_{i}");
            so.FindProperty("material").objectReferenceValue = Mat(UnlitMat);
            so.ApplyModifiedPropertiesWithoutUndo();
            go.AddComponent<FeedbackPlayer>();

            var cam = Camera.main;
            if (cam != null && cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();
        }

        static GameObject BuildEmberBurst()
        {
            var go = new GameObject("EmberBurst");
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.4f);
            main.startColor = new ParticleSystem.MinMaxGradient(M1Builder.Hex("#FF8A3D"), M1Builder.Hex("#FFCC66"));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 6) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            SpriteSheet(ps, M1Builder.LoadSprite("fireball.png"));
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Mat(UnlitMat);
            r.sortingOrder = 3;
            return Save(go, "EmberBurst");
        }

        static GameObject BuildSteamRing()
        {
            var go = new GameObject("SteamRing");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = JuiceSprite("steam_ring");
            sr.sharedMaterial = Mat(UnlitMat);
            sr.sortingOrder = 4;
            go.AddComponent<ExpandFade>();
            return Save(go, "SteamRing");
        }

        static GameObject BuildDeathTulang()
        {
            var go = new GameObject("DeathTulang");
            var fx = go.AddComponent<DeathEffect>();

            var shards = new SpriteRenderer[8];
            var rng = new System.Random(8);
            for (int i = 0; i < shards.Length; i++)
            {
                var s = new GameObject($"Shard{i}").AddComponent<SpriteRenderer>();
                s.transform.SetParent(go.transform, false);
                s.transform.localPosition = new Vector3((float)(rng.NextDouble() - 0.5) * 0.4f, (float)(rng.NextDouble() - 0.5) * 0.4f, 0f);
                s.sprite = JuiceSprite($"bone_shard_{i % 3}");
                s.sharedMaterial = Mat(LitMat);
                s.sortingOrder = 1;
                shards[i] = s;
            }

            var puffGo = new GameObject("AshPuff");
            puffGo.transform.SetParent(go.transform, false);
            var puff = puffGo.AddComponent<ParticleSystem>();
            puff.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = puff.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 1.6f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = puff.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8) });
            var shape = puff.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.2f;
            SpriteSheet(puff, JuiceSprite("ash_puff"));
            var pr = puffGo.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = Mat(LitMat);
            pr.sortingOrder = 2;

            // Kilatan visual saja: tanpa LightSourceBehaviour, jadi tidak ikut aturan Tersingkap.
            var flareGo = new GameObject("Flare");
            flareGo.transform.SetParent(go.transform, false);
            var flare = flareGo.AddComponent<Light2D>();
            flare.lightType = Light2D.LightType.Point;
            flare.color = M1Builder.Hex("#FF8A3D");
            flare.intensity = 1f;
            flare.pointLightInnerRadius = 0f;
            flare.pointLightOuterRadius = 1.5f;
            flare.shadowsEnabled = false;

            var eyes = new GameObject("Eyes").AddComponent<SpriteRenderer>();
            eyes.transform.SetParent(go.transform, false);
            eyes.sprite = M1Builder.LoadSprite("skeleton_eyes_0.png");
            eyes.sharedMaterial = Mat(UnlitMat);
            eyes.sortingOrder = 2;

            var so = new SerializedObject(fx);
            var arr = so.FindProperty("shards");
            arr.arraySize = shards.Length;
            for (int i = 0; i < shards.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = shards[i];
            so.FindProperty("ashPuff").objectReferenceValue = puff;
            so.FindProperty("flare").objectReferenceValue = flare;
            so.FindProperty("eyes").objectReferenceValue = eyes;
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go, "DeathTulang");
        }

        static GameObject BuildAshPile()
        {
            var go = new GameObject("AshPile");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = JuiceSprite("ash_pile");
            sr.sharedMaterial = Mat(LitMat);
            sr.sortingOrder = -6;
            var embers = new GameObject("Embers").AddComponent<SpriteRenderer>();
            embers.transform.SetParent(go.transform, false);
            embers.sprite = JuiceSprite("ash_embers");
            embers.sharedMaterial = Mat(UnlitMat);
            embers.sortingOrder = -5;
            embers.gameObject.AddComponent<EmberFlicker>();
            return Save(go, "AshPile");
        }

        static void SpriteSheet(ParticleSystem ps, Sprite sprite)
        {
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Sprites;
            sheet.AddSprite(sprite);
        }

        static Material FlashMaterial()
        {
            const string path = MaterialDir + "SpriteFlash.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Runecrypt/SpriteFlash"));
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static FeedbackProfile Profile(string name, System.Action<FeedbackProfile> init) =>
            M1ActorBuilder.LoadOrCreate(FeedbackDir + name + ".asset", init);

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static Sprite JuiceSprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(JuiceArt + name + ".png");
        static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + name + ".wav");
        static Material Mat(string path) => AssetDatabase.LoadAssetAtPath<Material>(path);
    }
}
