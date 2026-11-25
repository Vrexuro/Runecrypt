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
            // M2.6: tembakan dan pukulan lebih heboh (spec M2.6 §5.4). Diset ulang setiap build karena
            // LoadOrCreate di atas hanya mengisi nilai saat aset pertama kali dibuat.
            var muzzle = BuildMuzzleFlash();
            var hitBurst = BuildHitBurst();
            a.shot.particlePrefab = muzzle;
            a.hitLit.particlePrefab = hitBurst;
            a.hitDark.particlePrefab = hitBurst;
            foreach (var p in new[] { a.shot, a.hitLit, a.hitDark }) EditorUtility.SetDirty(p);
            AssetDatabase.SaveAssets();
            return a;
        }

        // ---------- M2.6: efek heboh yang tetap terbaca ----------
        // Efek pemain di sorting order <= 5, visual <= 0,4 dtk; mata musuh di order 10 (EyesOrder).
        public const int EyesOrder = 10;

        // Kilatan visual: Light2D tanpa bayangan dan tanpa LightSourceBehaviour (tidak ikut aturan cahaya).
        internal static Light2D AddFlare(Transform parent, string hex, float radius, float intensity, float duration)
        {
            var go = new GameObject("Flare");
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = M1Builder.Hex(hex);
            light.intensity = intensity;
            light.pointLightInnerRadius = 0f;
            light.pointLightOuterRadius = radius;
            light.shadowsEnabled = false;
            var fade = go.AddComponent<FlareFade>();
            var so = new SerializedObject(fade);
            so.FindProperty("duration").floatValue = duration;
            so.ApplyModifiedPropertiesWithoutUndo();
            return light;
        }

        // Semburan bara sekali pakai (umur <= 0,4 dtk).
        internal static ParticleSystem AddSparks(GameObject go, int count, float minSpeed, float maxSpeed, float maxLife, float maxSize)
        {
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 0.3f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(maxLife * 0.6f, maxLife);
            main.startSpeed = new ParticleSystem.MinMaxCurve(minSpeed, maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(maxSize * 0.6f, maxSize);
            main.startColor = new ParticleSystem.MinMaxGradient(M1Builder.Hex("#FF8A3D"), M1Builder.Hex("#FFE296"));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            SpriteSheet(ps, M1Builder.LoadSprite("fireball.png"));
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Mat(UnlitMat);
            r.sortingOrder = 3;
            return ps;
        }

        static GameObject BuildMuzzleFlash()
        {
            var go = new GameObject("MuzzleFlash");
            AddSparks(go, 4, 1.5f, 3f, 0.18f, 0.25f);
            AddFlare(go.transform, "#FFCC66", 1.25f, 1.6f, 0.08f);
            return Save(go, "MuzzleFlash");
        }

        static GameObject BuildHitBurst()
        {
            var go = new GameObject("HitBurst");
            AddSparks(go, 10, 2.5f, 5f, 0.35f, 0.45f);
            AddFlare(go.transform, "#FF9A4A", 1.6f, 1.8f, 0.12f);
            return Save(go, "HitBurst");
        }

        internal static GameObject BuildBlazeSparks()
        {
            var go = new GameObject("BlazeSparks");
            var ps = AddSparks(go, 18, 5f, 8f, 0.35f, 0.4f);
            var shape = ps.shape;
            shape.radius = 0.5f;
            return Save(go, "BlazeSparks");
        }

        // Ekor bara bola api: partikel per jarak, umur pendek.
        internal static void AddTrail(GameObject fireball)
        {
            var go = new GameObject("Trail");
            go.transform.SetParent(fireball.transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.25f);
            main.startColor = new ParticleSystem.MinMaxGradient(M1Builder.Hex("#FF8A3D"), M1Builder.Hex("#FFCC66"));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;
            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 12f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var shape = ps.shape;
            shape.enabled = false;
            SpriteSheet(ps, M1Builder.LoadSprite("fireball.png"));
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = Mat(UnlitMat);
            r.sortingOrder = 1;
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
            eyes.sortingOrder = EyesOrder;

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
