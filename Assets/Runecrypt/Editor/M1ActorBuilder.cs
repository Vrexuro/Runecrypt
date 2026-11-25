using System.IO;
using System.Linq;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runecrypt.EditorTools
{
    // Data, prefab, dan aktor M1 (Task 8). Jalankan setelah "1 Build Room", atau pakai "Build All".
    public static class M1ActorBuilder
    {
        const string DataDir = "Assets/Runecrypt/Data/";
        const string PrefabDir = "Assets/Runecrypt/Prefabs/";

        [MenuItem("Runecrypt/M1/Build All")]
        public static void BuildAll()
        {
            M1Builder.BuildRoom();
            BuildActors();
        }

        [MenuItem("Runecrypt/M1/2 Build Actors")]
        public static void BuildActors()
        {
            Directory.CreateDirectory(DataDir + "Enemies");
            Directory.CreateDirectory(PrefabDir);
            ConfigureCollisionMatrix();
            // Play mode tetap jalan saat editor tidak fokus (dikendalikan lewat MCP); di booth tidak berpengaruh.
            PlayerSettings.runInBackground = true;

            var rules = LoadOrCreate<LightRules>(DataDir + "LightRules.asset", _ => { });
            var tulang = LoadOrCreate<EnemyDefinition>(DataDir + "Enemies/TulangPenjaga.asset", d =>
            {
                d.displayName = "Tulang Penjaga";
                d.maxHp = 12f; d.moveSpeed = 2f; d.contactDamage = 1f; d.contactInterval = 1f; d.element = Element.None;
            });

            // M1.5: profil umpan balik dan prefab efek lebih dulu, karena prefab aktor menunjuknya.
            var juice = M1JuiceBuilder.BuildAssets();
            tulang.hitLit = juice.hitLit;
            tulang.hitDark = juice.hitDark;
            tulang.deathProfile = juice.deathTulang;
            tulang.deathEffect = juice.deathTulangFx;
            tulang.remains = juice.ashPile;
            EditorUtility.SetDirty(tulang);

            // M2: relic, Kobaran, pickup, dan material garis tepi.
            var m2 = M2ContentBuilder.BuildAssets(juice.shot);

            var fireball = BuildFireballPrefab();
            var player = BuildPlayerPrefab(fireball, juice, m2);
            var enemy = BuildEnemyPrefab(tulang, rules, m2);

            // Task 9: Uap + Genangan Beku.
            Directory.CreateDirectory(DataDir + "Reactions");
            var steam = BuildSteamPrefab();
            var uap = LoadOrCreate<ReactionDefinition>(DataDir + "Reactions/Uap.asset", d =>
            {
                d.displayName = "Uap";
                d.elementA = Element.Fire; d.elementB = Element.Ice;
                d.radius = 2.5f; d.duration = 3f; d.damagePerTick = 2f; d.tickInterval = 0.5f; d.blindDuration = 0.6f;
            });
            uap.effectPrefab = steam;
            uap.spawnProfile = juice.steam;
            EditorUtility.SetDirty(uap);
            var puddle = BuildPuddlePrefab(uap);

            foreach (var name in new[] { "TestLight", "SkeletonDummy", "Player", "TulangPenjaga", "FrostPuddle", "ArenaDirector", "Feedback" })
                foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(g => g != null && g.name == name && g.transform.parent == null).ToList())
                    Object.DestroyImmediate(go);

            Place(player, M1Builder.RoomCenter);
            Place(puddle, new Vector3(-4f, -5.5f, 0f));
            Place(puddle, new Vector3(9.5f, -3.5f, 0f));
            BuildArenaDirector(enemy, m2);
            M1JuiceBuilder.BuildSceneServices();
            // M2.5: kit makam (lantai, dinding, dekorasi, Anglo, Guci, nisan, debu, ambien).
            M25CryptBuilder.DressArena(M25CryptBuilder.BuildAssets());

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[M1ActorBuilder] Actors built.");
        }

        static void ConfigureCollisionMatrix()
        {
            int player = Layer("Player"), enemies = Layer("Enemies"), proj = Layer("Projectiles"), react = Layer("Reactives"), walls = Layer("Walls");
            Physics2D.IgnoreLayerCollision(proj, player, true);
            Physics2D.IgnoreLayerCollision(proj, proj, true);
            Physics2D.IgnoreLayerCollision(react, player, true);
            Physics2D.IgnoreLayerCollision(react, enemies, true);
            Physics2D.IgnoreLayerCollision(react, walls, true);
            Physics2D.IgnoreLayerCollision(react, react, true);

            // M2.5: Anglo dan Guci padat bagi pemain, musuh, dan tembakan; uap dan dinding tidak menyentuhnya.
            int props = EnsureLayer("Props");
            Physics2D.IgnoreLayerCollision(props, react, true);
            Physics2D.IgnoreLayerCollision(props, walls, true);
        }

        // Menambah layer di slot user pertama yang kosong (8–31) kalau belum ada.
        static int EnsureLayer(string name)
        {
            int existing = Layer(name);
            if (existing >= 0) return existing;
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tags.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                if (!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue)) continue;
                layers.GetArrayElementAtIndex(i).stringValue = name;
                tags.ApplyModifiedPropertiesWithoutUndo();
                return i;
            }
            throw new System.InvalidOperationException("Tidak ada slot layer kosong untuk " + name);
        }

        static Fireball BuildFireballPrefab()
        {
            var go = new GameObject("Fireball") { layer = Layer("Projectiles") };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = M1Builder.LoadSprite("fireball.png");
            sr.sortingOrder = 2;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
            go.AddComponent<Fireball>();
            M1Builder.MakePointLight("Light", go.transform, "#FFCC66", 1f);
            return SavePrefab(go, "Fireball").GetComponent<Fireball>();
        }

        static GameObject BuildPlayerPrefab(Fireball fireball, M1JuiceBuilder.Assets juice, M2ContentBuilder.Assets m2)
        {
            var go = new GameObject("Player") { layer = Layer("Player") };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = M1Builder.LoadSprite("mage_0.png");
            sr.sortingOrder = 1;
            SetSprites(go.AddComponent<SpriteFlipbook>(), "mage_");
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.mass = 50f; // musuh (massa 1) tidak bisa mendorong pemain
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            var pc = go.AddComponent<PlayerController>();
            Set(pc, "fireballPrefab", fireball);
            var light = M1Builder.MakePointLight("Light", go.transform, "#FF8A3D", 3f);
            Set(pc, "shotProfile", juice.shot);
            Set(pc, "hurtProfile", juice.playerHurt);
            Set(pc, "hitFlash", M1JuiceBuilder.AddHitFlash(go));
            Set(pc, "lightPulse", light.gameObject.AddComponent<LightPulse>());
            Set(pc, "bodySprite", sr);
            Set(pc, "lightSource", light.GetComponent<LightSourceBehaviour>());
            Set(pc, "blazePrefab", m2.blaze);
            return SavePrefab(go, "Player");
        }

        static GameObject BuildEnemyPrefab(EnemyDefinition def, LightRules rules, M2ContentBuilder.Assets m2)
        {
            var go = new GameObject("TulangPenjaga") { layer = Layer("Enemies") };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = M1Builder.LoadSprite("skeleton_0.png");
            SetSprites(go.AddComponent<SpriteFlipbook>(), "skeleton_");
            var eyes = M1Builder.AddEyes(go.transform, "skeleton_eyes_0.png");
            SetSprites(eyes.gameObject.AddComponent<SpriteFlipbook>(), "skeleton_eyes_");
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var burn = M1Builder.MakePointLight("BurnLight", go.transform, "#FF8A3D", 0.75f);
            burn.shadowsEnabled = false;
            var enemy = go.AddComponent<Enemy>();
            Set(enemy, "definition", def);
            Set(enemy, "lightRules", rules);
            Set(enemy, "burnLight", burn.gameObject);
            Set(enemy, "hitFlash", M1JuiceBuilder.AddHitFlash(go));
            M2ContentBuilder.AddEnemyParts(go, enemy);
            return SavePrefab(go, "TulangPenjaga");
        }

        // Task 10: gelombang musuh dari 6 titik di sudut gelap ruangan.
        static void BuildArenaDirector(GameObject enemyPrefab, M2ContentBuilder.Assets m2)
        {
            var go = new GameObject("ArenaDirector");
            var director = go.AddComponent<ArenaDirector>();
            Vector3[] points =
            {
                // M2.6: di dekat keempat tepi arena 30x17, sebagian di luar tampilan awal kamera.
                new Vector3(-13f, 5f), new Vector3(13f, 5f), new Vector3(-13f, -7f), new Vector3(13f, -7f),
                new Vector3(0f, 5.5f), new Vector3(0f, -7.5f), new Vector3(-13f, -1f), new Vector3(13f, -1f),
            };
            var so = new SerializedObject(director);
            so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab.GetComponent<Enemy>();
            var arr = so.FindProperty("spawnPoints");
            arr.arraySize = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                var sp = new GameObject($"Spawn{i}").transform;
                sp.SetParent(go.transform, false);
                sp.position = points[i];
                arr.GetArrayElementAtIndex(i).objectReferenceValue = sp;
            }
            var pool = so.FindProperty("relicPool");
            pool.arraySize = m2.relics.Count;
            for (int i = 0; i < m2.relics.Count; i++) pool.GetArrayElementAtIndex(i).objectReferenceValue = m2.relics[i];
            so.FindProperty("relicPickupPrefab").objectReferenceValue = m2.pickup;
            so.FindProperty("pickupPoint").vector2Value = M1Builder.RoomCenter;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject BuildSteamPrefab()
        {
            var go = new GameObject("SteamCloud");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 0.3f;
            main.startSize = new ParticleSystem.MinMaxCurve(1f, 1.6f);
            main.startColor = new Color(1f, 1f, 1f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 25f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 2f;
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = true;
            sheet.mode = ParticleSystemAnimationMode.Sprites;
            sheet.AddSprite(M1Builder.LoadSprite("steam_puff.png"));
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Lit-Default.mat");
            psr.sortingOrder = 3;

            var light = M1Builder.MakePointLight("Light", go.transform, "#D9D1BD", 2.5f);
            light.shadowsEnabled = false;
            var cloud = go.AddComponent<SteamCloud>();
            Set(cloud, "lightSource", light.GetComponent<LightSourceBehaviour>());
            return SavePrefab(go, "SteamCloud");
        }

        static GameObject BuildPuddlePrefab(ReactionDefinition uap)
        {
            var go = new GameObject("FrostPuddle") { layer = Layer("Reactives") };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = M1Builder.LoadSprite("frost_puddle.png");
            sr.sortingOrder = -5;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.5f);
            box.offset = new Vector2(0f, -0.1f);
            var fp = go.AddComponent<FrostPuddle>();
            var so = new SerializedObject(fp);
            var book = so.FindProperty("reactionBook");
            book.arraySize = 1;
            book.GetArrayElementAtIndex(0).objectReferenceValue = uap;
            so.ApplyModifiedPropertiesWithoutUndo();
            return SavePrefab(go, "FrostPuddle");
        }

        static void SetSprites(SpriteFlipbook fb, string prefix)
        {
            var so = new SerializedObject(fb);
            var arr = so.FindProperty("frames");
            arr.arraySize = 4;
            for (int i = 0; i < 4; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = M1Builder.LoadSprite($"{prefix}{i}.png");
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void Place(GameObject prefab, Vector3 pos)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = pos;
        }

        internal static T LoadOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static int Layer(string name) => LayerMask.NameToLayer(name);
    }
}
