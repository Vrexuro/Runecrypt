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

            var fireball = BuildFireballPrefab();
            var player = BuildPlayerPrefab(fireball);
            var enemy = BuildEnemyPrefab(tulang, rules);

            foreach (var name in new[] { "TestLight", "SkeletonDummy", "Player", "TulangPenjaga" })
                foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(g => g != null && g.name == name && g.transform.parent == null).ToList())
                    Object.DestroyImmediate(go);

            Place(player, M1Builder.RoomCenter);
            Place(enemy, new Vector3(6f, 2f, 0f));
            Place(enemy, new Vector3(-7f, -3.5f, 0f));

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

        static GameObject BuildPlayerPrefab(Fireball fireball)
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
            M1Builder.MakePointLight("Light", go.transform, "#FF8A3D", 3f);
            return SavePrefab(go, "Player");
        }

        static GameObject BuildEnemyPrefab(EnemyDefinition def, LightRules rules)
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
            return SavePrefab(go, "TulangPenjaga");
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

        static T LoadOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
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
