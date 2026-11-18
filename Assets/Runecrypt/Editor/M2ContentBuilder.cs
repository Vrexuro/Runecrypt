using System.Collections.Generic;
using System.IO;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.EditorTools
{
    // M2 (Docs/plans/2026-11-18-m2-mesin-konten.md Task 9): 10 relic data, prefab Kobaran dan pickup relic,
    // serta anak-anak musuh (ikon status, penanda homing).
    // Dipanggil dari M1ActorBuilder supaya "Runecrypt/M1/Build All" tetap mereproduksi scene.
    public static class M2ContentBuilder
    {
        const string RelicDir = "Assets/Runecrypt/Data/Relics/";
        const string PrefabDir = "Assets/Runecrypt/Prefabs/";
        const string JuiceArt = "Assets/Runecrypt/Art/Juice/";
        const string UnlitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        public class Assets
        {
            public List<RelicDefinition> relics;
            public BlazeRing blaze;
            public RelicPickup pickup;
        }

        public static Assets BuildAssets(FeedbackProfile pickupProfile)
        {
            Directory.CreateDirectory(RelicDir);
            var a = new Assets
            {
                relics = BuildRelics(),
                blaze = BuildBlaze(),
                pickup = BuildPickup(pickupProfile),
            };
            AssetDatabase.SaveAssets();
            return a;
        }

        // GDD §5.2 relic 1–10. Relic 11–15 butuh perilaku baru di luar stat, menyusul di M3/M4.
        static List<RelicDefinition> BuildRelics() => new List<RelicDefinition>
        {
            Relic("BaraKembar", "Bara Kembar", "Setiap tembakan mengeluarkan 2 bola api yang menyebar 10°.", false,
                M(Stat.ProjectileCount, StatOp.Add, 1f)),
            Relic("TaringObsidian", "Taring Obsidian", "Bola api menembus 1 musuh tambahan.", false,
                M(Stat.Pierce, StatOp.Add, 1f)),
            Relic("CerminRetak", "Cermin Retak", "Bola api memantul dari dinding 2 kali.", false,
                M(Stat.Bounce, StatOp.Add, 2f)),
            Relic("MataNgengat", "Mata Ngengat", "Bola api berbelok perlahan ke musuh terang terdekat.", false,
                M(Stat.HomingTurn, StatOp.Add, 180f)),
            Relic("SumbuPanjang", "Sumbu Panjang", "Jangkauan +30%, cahaya bola api lebih lebar.", false,
                M(Stat.Range, StatOp.Percent, 0.3f), M(Stat.ProjectileLight, StatOp.Percent, 0.3f)),
            Relic("LilinGanda", "Lilin Ganda", "Laju tembak +20%.", false,
                M(Stat.FireRate, StatOp.Percent, 0.2f)),
            Relic("AbuPekat", "Abu Pekat", "Durasi Terbakar +2 detik.", false,
                M(Stat.BurnDuration, StatOp.Add, 2f)),
            Relic("ArangBerat", "Arang Berat", "Bola api +40% lebih besar dan damage +20%, tapi 15% lebih lambat.", false,
                M(Stat.ProjectileSize, StatOp.Percent, 0.4f), M(Stat.Damage, StatOp.Percent, 0.2f),
                M(Stat.ProjectileSpeed, StatOp.Percent, -0.15f)),
            Relic("LenteraTulang", "Lentera Tulang", "Radius cahaya pemain +25%.", false,
                M(Stat.LightRadius, StatOp.Percent, 0.25f)),
            Relic("JantungTungku", "Jantung Tungku", "Max HP +2 (satu hati) dan pulih penuh.", true,
                M(Stat.MaxHp, StatOp.Add, 2f)),
        };

        static StatMod M(Stat s, StatOp op, float v) => new StatMod { stat = s, op = op, value = v };

        static RelicDefinition Relic(string file, string name, string desc, bool healFull, params StatMod[] mods) =>
            M1ActorBuilder.LoadOrCreate<RelicDefinition>(RelicDir + file + ".asset", r =>
            {
                r.displayName = name;
                r.description = desc;
                r.icon = Sprite("relic_orb");
                r.mods = mods;
                r.healFull = healFull;
            });

        // Kobaran: cincin yang mengembang ke diameter 5 (radius 2,5) dan lampu yang menyala 0,5 detik.
        static BlazeRing BuildBlaze()
        {
            var go = new GameObject("BlazeRing");
            var ring = new GameObject("Ring");
            ring.transform.SetParent(go.transform, false);
            var sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite("blaze_ring");
            sr.sharedMaterial = Mat(UnlitMat);
            sr.sortingOrder = 4;
            var fade = ring.AddComponent<ExpandFade>();
            var so = new SerializedObject(fade);
            so.FindProperty("duration").floatValue = 0.25f;
            so.FindProperty("targetScale").floatValue = 5f / 3f; // sprite 48 px = 3 unit
            so.ApplyModifiedPropertiesWithoutUndo();

            var light = M1Builder.MakePointLight("Light", go.transform, "#FF8A3D", 3.5f);
            light.gameObject.SetActive(false); // dinyalakan BlazeRing.Init sebelum damage
            var blaze = go.AddComponent<BlazeRing>();
            Set(blaze, "lightSource", light.GetComponent<LightSourceBehaviour>());
            return Save(go, "BlazeRing").GetComponent<BlazeRing>();
        }

        static RelicPickup BuildPickup(FeedbackProfile profile)
        {
            var go = new GameObject("RelicPickup");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite("relic_orb");
            sr.sharedMaterial = Mat(UnlitMat);
            sr.sortingOrder = 1;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            var pickup = go.AddComponent<RelicPickup>();
            Set(pickup, "pickupProfile", profile);
            M1Builder.MakePointLight("Light", go.transform, "#FFCC66", 1f).shadowsEnabled = false;
            return Save(go, "RelicPickup").GetComponent<RelicPickup>();
        }

        // Anak musuh M2. Dipanggil dari M1ActorBuilder.BuildEnemyPrefab sebelum prefab disimpan.
        public static void AddEnemyParts(GameObject go, Enemy enemy)
        {
            Set(enemy, "blindIcon", Icon(go, "BlindIcon", "icon_blind", new Vector2(0f, 0.75f)));
            Set(enemy, "stunIcon", Icon(go, "StunIcon", "icon_stun", new Vector2(0f, 0.75f)));
            Set(enemy, "homingMarker", Icon(go, "HomingMarker", "homing_marker", new Vector2(0.45f, 0.45f)));
        }

        static GameObject Icon(GameObject parent, string name, string sprite, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite(sprite);
            sr.sharedMaterial = Mat(UnlitMat);
            sr.sortingOrder = 6;
            go.SetActive(false);
            return go;
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabDir + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }

        static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(JuiceArt + name + ".png");
        static Material Mat(string path) => AssetDatabase.LoadAssetAtPath<Material>(path);
    }
}
