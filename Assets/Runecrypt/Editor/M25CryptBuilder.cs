using System.Collections.Generic;
using System.IO;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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

        const string AudioDir = "Assets/Runecrypt/Audio/";

        static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(AudioDir + name + ".wav");

        // Klip dipasang ulang setiap build (dari make_sfx_cc0.py, Task 8), nilai lain hanya saat aset dibuat.
        static FeedbackProfile Profile(string name, float shake, float volume, string clip)
        {
            var p = M1ActorBuilder.LoadOrCreate<FeedbackProfile>(FeedbackDir + name + ".asset", x =>
            {
                x.shakeTrauma = shake;
                x.volume = volume;
            });
            var c = Clip(clip);
            p.clips = c != null ? new[] { c } : new AudioClip[0];
            EditorUtility.SetDirty(p);
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
            so.FindProperty("pickupProfile").objectReferenceValue = Profile("EmberPickup", 0f, 0.8f, "sfx_ember_pickup");
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
            var breakProfile = Profile("UrnBreak", 0.05f, 0.8f, "sfx_urn_break");
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

            var profile = Profile("BrazierIgnite", 0.1f, 0.8f, "sfx_brazier_ignite");
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

        // ---------- Task 7: menata arena M1/M2 ----------
        const string TilesDir = "Assets/Runecrypt/Data/Tiles/";
        const string LitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat";
        const int DecorOrder = -8, WallOverlayOrder = 1, Seed = 2511;
        const float TopWallY = 4.5f;     // baris dinding atas (sel y = 4)

        // Bobot ubin lantai: polos dominan (~70%, spec §5.1).
        static readonly (string name, int weight)[] FloorWeights =
            { ("plain", 14), ("cracked", 2), ("moss", 2), ("bones", 1), ("rune", 1) };

        // Posisi di pinggir dan sudut, menjauhi spawn point dan posisi awal pemain (spec §6, Review Focus 5).
        static readonly Vector2[] BrazierAt = { new Vector2(-8.5f, 0.5f), new Vector2(7.5f, -0.5f) };
        static readonly (Vector2 pos, string sprite)[] UrnAt =
            { (new Vector2(-5.5f, -4.5f), "urn_a"), (new Vector2(-4.5f, -4.5f), "urn_b"), (new Vector2(5.5f, 3.5f), "urn_a") };
        static readonly Vector2[] TombstoneAt = { new Vector2(-2.5f, 2.5f), new Vector2(2.5f, -3.5f) };
        static readonly Vector2 SarcophagusAt = new Vector2(-5f, 2.5f);
        static readonly (float x, string sprite)[] WallOverlays =
        {
            (-6.5f, "wall_niche"), (-3.5f, "wall_torch_dead"), (-0.5f, "wall_niche"), (2.5f, "wall_torch_dead"),
            (5.5f, "wall_niche"), (7.5f, "wall_cracked"), (-8.5f, "wall_cracked"),
        };
        static readonly (Vector2 pos, string sprite)[] DecorAt =
        {
            (new Vector2(-9.5f, 3.5f), "decor_web_l"), (new Vector2(8.5f, 3.5f), "decor_web_r"),
            (new Vector2(-8.5f, -5.5f + 1f), "decor_web_l"), (new Vector2(-3.5f, 3.5f), "decor_chain"),
            (new Vector2(1.5f, 3.5f), "decor_chain"), (new Vector2(3.5f, 3.5f), "decor_banner"),
            (new Vector2(-7.5f, -1.5f), "decor_bones"), (new Vector2(3.5f, -4.5f), "decor_bones"),
            (new Vector2(6.5f, 2.5f), "decor_candle"), (new Vector2(-1.5f, -4.5f), "decor_candle"),
        };

        // Tidak menyimpan: penataan ditandai sebagai perubahan scene, user (atau "Build All") yang menyimpannya.
        // Dengan begitu test yang memanggil menu ini tidak menulis ulang scene yang di-commit (review akhir Important 1).
        [MenuItem("Runecrypt/M2.5/Dress Arena")]
        static void DressArenaMenu()
        {
            DressArena(BuildAssets());
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        // Idempoten: root CryptKit dihapus lalu dibuat ulang; ubin dipilih dengan seed tetap.
        public static void DressArena(Assets a)
        {
            DressFloor();
            DressPillars(a.pillarSprite);

            var old = GameObject.Find("CryptKit");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("CryptKit").transform;
            var decor = Child(root, "Decor");
            var props = Child(root, "Props");

            foreach (var (x, sprite) in WallOverlays) Decor(decor, sprite, new Vector2(x, TopWallY), WallOverlayOrder);
            foreach (var (pos, sprite) in DecorAt) Decor(decor, sprite, pos, DecorOrder);

            foreach (var p in BrazierAt) Place(a.brazier.gameObject, props, p);
            foreach (var (pos, sprite) in UrnAt)
            {
                var urn = Place(a.urn.gameObject, props, pos);
                var so = new SerializedObject(urn.GetComponent<AshUrn>());
                so.FindProperty("wholeSprite").objectReferenceValue = Sprite(sprite);
                so.ApplyModifiedPropertiesWithoutUndo();
                urn.GetComponent<SpriteRenderer>().sprite = Sprite(sprite);
            }
            foreach (var p in TombstoneAt) Place(a.tombstone, props, p);
            Place(a.sarcophagus, props, SarcophagusAt);

            BuildDust(Child(root, "Dust"));
            var amb = Child(root, "Ambience").gameObject.AddComponent<AudioSource>();
            amb.loop = true;
            amb.playOnAwake = true;
            amb.volume = 0.25f; // jauh di bawah SFX (spec §5.1)
            amb.clip = Clip("amb_crypt_loop");
            Debug.Log("[M25CryptBuilder] Arena ditata.");
        }

        static void DressFloor()
        {
            var floor = GameObject.Find("Grid/Floor").GetComponent<Tilemap>();
            var tiles = new List<(Tile tile, int weight)>();
            int sum = 0;
            foreach (var (name, w) in FloorWeights)
            {
                tiles.Add((CryptTile("CryptFloor_" + name, "floor_" + name), w));
                sum += w;
            }
            var rng = new System.Random(Seed);
            foreach (var cell in floor.cellBounds.allPositionsWithin)
            {
                if (floor.GetTile(cell) == null) continue;
                int roll = rng.Next(sum);
                foreach (var (tile, w) in tiles)
                {
                    if (roll < w) { floor.SetTile(cell, tile); break; }
                    roll -= w;
                }
            }
        }

        static Tile CryptTile(string asset, string sprite)
        {
            Directory.CreateDirectory(TilesDir);
            string path = TilesDir + asset + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = Sprite(sprite);
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // Pilar lama digambar dengan tile dinding di tilemap Obstacles; diganti satu sprite pilar 2x2 per PillarBody.
        static void DressPillars(Sprite pillar)
        {
            var obstacles = GameObject.Find("Grid/Obstacles").GetComponent<Tilemap>();
            obstacles.ClearAllTiles();
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name != "PillarBody") continue;
                var existing = go.transform.Find("PillarSprite");
                var sr = existing != null ? existing.GetComponent<SpriteRenderer>()
                    : Child(go.transform, "PillarSprite").gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = pillar;
                sr.sortingOrder = 0;
            }
        }

        static void BuildDust(Transform t)
        {
            t.position = M1Builder.RoomCenter;
            var ps = t.gameObject.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
            main.startSize = 0.125f;   // 2 piksel
            main.maxParticles = 40;    // ringan untuk laptop booth (spec §2)
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(1f, 1f, 1f, 0.6f);
            var emission = ps.emission;
            emission.rateOverTime = 6f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18f, 9f, 0f);
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.08f;
            noise.frequency = 0.3f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Sprites;
            sheet.AddSprite(Sprite("dust"));
            var r = t.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(LitMat); // Sprite-Lit: debu hanya tampak di area terang
            r.sortingOrder = 3;
        }

        static Transform Child(Transform parent, string name)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        static void Decor(Transform parent, string sprite, Vector2 pos, int order)
        {
            var t = Child(parent, sprite);
            t.position = pos;
            var sr = t.gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite(sprite);
            sr.sortingOrder = order;
        }

        static GameObject Place(GameObject prefab, Transform parent, Vector2 pos)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
            return go;
        }

        static GameObject Save(GameObject go)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + go.name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
