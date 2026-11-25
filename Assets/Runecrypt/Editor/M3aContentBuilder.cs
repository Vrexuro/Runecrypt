using System.Collections.Generic;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.EditorTools
{
    // M3a (Docs/plans/2026-11-18-m3a-musuh-es.md): Pelita Sesat, Biarawan Beku, serpih es, Kristal Es, efek mati.
    // Sprite dari codeart2d (ArtSource/codeart/, diekspor oleh ArtSource/scripts/export_codeart.py).
    public static class M3aContentBuilder
    {
        const string Art = "Assets/Runecrypt/Art/Enemies/";
        const string DataDir = "Assets/Runecrypt/Data/Enemies/";
        const string PrefabDir = "Assets/Runecrypt/Prefabs/";
        const string UnlitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const int ShardOrder = 9; // di atas efek pemain (<= 5), di bawah mata musuh (10); spec M2.6 §5.4

        public class Assets
        {
            public GameObject pelita, biarawan, crystal;
        }

        public static Assets Build(M1JuiceBuilder.Assets juice, LightRules rules, ReactionDefinition uap, GameObject puddle)
        {
            var deathPelita = BuildDeathFx("DeathPelita", "pelita_death_", 3);
            var deathBiarawan = BuildDeathFx("DeathBiarawan", "biarawan_death_", 3);

            // Angka keputusan Claude, disetel saat playtest (rencana M3a Task 3 dan 4). Diset ulang setiap build
            // karena LoadOrCreate hanya mengisi nilai saat aset pertama kali dibuat.
            var pelitaDef = M1ActorBuilder.LoadOrCreate<EnemyDefinition>(DataDir + "PelitaSesat.asset", _ => { });
            pelitaDef.displayName = "Pelita Sesat";
            pelitaDef.maxHp = 6f; pelitaDef.moveSpeed = 2.4f; pelitaDef.contactDamage = 1f; pelitaDef.contactInterval = 0.8f;
            pelitaDef.element = Element.None; pelitaDef.cost = 1;
            pelitaDef.deathEffect = deathPelita;
            Common(pelitaDef, juice);

            var monkDef = M1ActorBuilder.LoadOrCreate<EnemyDefinition>(DataDir + "BiarawanBeku.asset", _ => { });
            monkDef.displayName = "Biarawan Beku";
            monkDef.maxHp = 10f; monkDef.moveSpeed = 1.6f; monkDef.contactDamage = 1f; monkDef.contactInterval = 1f;
            monkDef.element = Element.Ice; monkDef.cost = 2; monkDef.chargeCooldown = 4f;
            monkDef.dropOnDeath = puddle;
            monkDef.deathEffect = deathBiarawan;
            Common(monkDef, juice);

            var shard = BuildShard();
            return new Assets
            {
                pelita = BuildPelita(pelitaDef, rules),
                biarawan = BuildBiarawan(monkDef, rules, uap, shard),
                crystal = BuildCrystal(uap),
            };
        }

        static void Common(EnemyDefinition d, M1JuiceBuilder.Assets juice)
        {
            d.hitLit = juice.hitLit;
            d.hitDark = juice.hitDark;
            d.deathProfile = juice.deathTulang;
            d.remains = null;
            EditorUtility.SetDirty(d);
        }

        static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + name + ".png");

        static Material Unlit => AssetDatabase.LoadAssetAtPath<Material>(UnlitMat);

        static SpriteFlipbook Flipbook(GameObject go, string prefix, int count, float fps, bool once = false)
        {
            var fb = go.AddComponent<SpriteFlipbook>();
            var so = new SerializedObject(fb);
            var arr = so.FindProperty("frames");
            arr.arraySize = count;
            for (int i = 0; i < count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = S(prefix + i);
            so.FindProperty("fps").floatValue = fps;
            so.FindProperty("playOnce").boolValue = once;
            so.ApplyModifiedPropertiesWithoutUndo();
            return fb;
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Kerangka musuh yang sama dengan Tulang Penjaga: tubuh lit, mata unlit di order 10, cahaya terbakar, kedipan.
        static (GameObject go, SpriteRenderer eyes) EnemyBody(string name, string body, string eyesSprite)
        {
            var go = new GameObject(name) { layer = LayerMask.NameToLayer("Enemies") };
            go.AddComponent<SpriteRenderer>().sprite = S(body + "0");
            Flipbook(go, body, 4, 6f);
            var eyes = M1Builder.AddEyes(go.transform, "-");
            eyes.sprite = S(eyesSprite);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            return (go, eyes);
        }

        static Enemy AddEnemy(GameObject go, EnemyDefinition def, LightRules rules, ReactionDefinition reaction)
        {
            var burn = M1Builder.MakePointLight("BurnLight", go.transform, "#FF8A3D", 0.75f);
            burn.shadowsEnabled = false;
            // Otak memakai [RequireComponent(typeof(Enemy))], jadi Enemy sudah ditambahkan otomatis.
            var enemy = go.GetComponent<Enemy>();
            if (enemy == null) enemy = go.AddComponent<Enemy>();
            Set(enemy, "definition", def);
            Set(enemy, "lightRules", rules);
            Set(enemy, "burnLight", burn.gameObject);
            Set(enemy, "hitFlash", M1JuiceBuilder.AddHitFlash(go));
            if (reaction != null)
            {
                var so = new SerializedObject(enemy);
                var book = so.FindProperty("reactionBook");
                book.arraySize = 1;
                book.GetArrayElementAtIndex(0).objectReferenceValue = reaction;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            M2ContentBuilder.AddEnemyParts(go, enemy);
            return enemy;
        }

        static GameObject BuildPelita(EnemyDefinition def, LightRules rules)
        {
            var (go, eyes) = EnemyBody("PelitaSesat", "pelita_sesat_", "pelita_eyes_0");
            var brain = go.AddComponent<SwoopBrain>();
            Set(brain, "eyes", eyes);
            Set(brain, "windupEyes", S("pelita_eyes_1"));
            AddEnemy(go, def, rules, null);
            return Save(go, "PelitaSesat");
        }

        static GameObject BuildBiarawan(EnemyDefinition def, LightRules rules, ReactionDefinition uap, EnemyShot shard)
        {
            var (go, eyes) = EnemyBody("BiarawanBeku", "biarawan_beku_", "biarawan_eyes_0");
            Flipbook(eyes.gameObject, "biarawan_eyes_", 4, 6f); // mata ikut naik-turun bersama jubah
            var brain = go.AddComponent<RangedBrain>();
            Set(brain, "shotPrefab", shard);
            AddEnemy(go, def, rules, uap);
            return Save(go, "BiarawanBeku");
        }

        static EnemyShot BuildShard()
        {
            var go = new GameObject("IceShard") { layer = LayerMask.NameToLayer("EnemyShots") };
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S("ice_shard_0");
            sr.sharedMaterial = Unlit;
            sr.sortingOrder = ShardOrder;
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            go.AddComponent<EnemyShot>();
            return Save(go, "IceShard").GetComponent<EnemyShot>();
        }

        static GameObject BuildCrystal(ReactionDefinition uap)
        {
            var go = new GameObject("IceCrystal") { layer = LayerMask.NameToLayer("Props") };
            go.AddComponent<SpriteRenderer>().sprite = S("ice_crystal_0");
            Flipbook(go, "ice_crystal_", 2, 2.5f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.8f);
            var prop = go.AddComponent<ReactiveProp>();
            var so = new SerializedObject(prop);
            so.FindProperty("element").enumValueIndex = (int)Element.Ice;
            var book = so.FindProperty("reactionBook");
            book.arraySize = 1;
            book.GetArrayElementAtIndex(0).objectReferenceValue = uap;
            so.FindProperty("singleUse").boolValue = true;
            so.FindProperty("shatterDamage").floatValue = 4f;   // keputusan Claude
            so.FindProperty("shatterRadius").floatValue = 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go, "IceCrystal");
        }

        // Efek mati sekali putar (asap Pelita, pecahan es Biarawan); unlit supaya mata dan serpih terlihat di gelap.
        static GameObject BuildDeathFx(string name, string prefix, int count)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S(prefix + "0");
            sr.sharedMaterial = Unlit;
            sr.sortingOrder = 4;
            Flipbook(go, prefix, count, 10f, once: true);
            return Save(go, name);
        }

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
