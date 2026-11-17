using System.IO;
using System.Linq;
using Runecrypt.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Runecrypt.EditorTools
{
    // Membangun ulang ruangan prototipe M1 dari nol, supaya scene bisa direproduksi.
    public static class M1Builder
    {
        public const string Art = "Assets/Runecrypt/Art/Prototype/";
        public const string ScenePath = "Assets/Runecrypt/Scenes/M1_Prototype.unity";
        const string TilesDir = "Assets/Runecrypt/Data/Tiles/";
        const string UnlitMat = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        // Ruangan 20x11 tile, sel (MinX..MinX+W-1, MinY..MinY+H-1); pusatnya di (0, -0.5).
        public const int W = 20, H = 11, MinX = -10, MinY = -6;
        public static readonly Vector3 RoomCenter = new Vector3(0f, -0.5f, 0f);
        static readonly RectInt[] Pillars = { new RectInt(-5, -2, 2, 2), new RectInt(3, 0, 2, 2) };

        [MenuItem("Runecrypt/M1/1 Build Room")]
        public static void BuildRoom()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            BuildCamera();

            var host = new GameObject("LightFieldHost").AddComponent<LightFieldHost>();
            var so = new SerializedObject(host);
            so.FindProperty("wallMask").intValue = LayerMask.GetMask("Walls");
            so.ApplyModifiedPropertiesWithoutUndo();

            BuildTilemaps();

            var global = new GameObject("Global Light 2D").AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.intensity = 0.08f;
            global.color = Hex("#342852");

            // Lampu uji Task 7; diganti cahaya pemain di Task 8.
            var test = MakePointLight("TestLight", null, "#FF8A3D", 3f);
            test.transform.position = RoomCenter;

            MakeSkeletonDummy(new Vector3(1.5f, -0.5f, 0f));
            MakeSkeletonDummy(new Vector3(7f, 2f, 0f));

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            var others = EditorBuildSettings.scenes.Where(s => s.path != ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }.Concat(others).ToArray();
            Debug.Log("[M1Builder] Room built: " + ScenePath);
        }

        static void BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(RoomCenter.x, RoomCenter.y, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f; // 180 px / 16 PPU / 2
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(14, 10, 20, 255);
            go.AddComponent<UniversalAdditionalCameraData>();
            var ppc = go.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = 16;
            ppc.refResolutionX = 320;
            ppc.refResolutionY = 180;
        }

        static void BuildTilemaps()
        {
            var floorTile = LoadOrCreateTile("Floor", "floor.png");
            var wallTile = LoadOrCreateTile("Wall", "wall.png");

            var grid = new GameObject("Grid").AddComponent<Grid>();
            var floor = MakeTilemap(grid, "Floor", -10);
            // Konvensi ruangan: "Walls" = dinding pinggir (collider saja), "Obstacles" = pilar di dalam
            // ruangan (collider + bayangan). ShadowCaster2D pada dinding cincin membaca garis luarnya
            // saja sehingga seluruh ruangan tertutup bayangan.
            var walls = MakeSolidTilemap(grid, "Walls");
            var obstacles = MakeTilemap(grid, "Obstacles", 0); // visual saja; collider + bayangan per pilar

            for (int x = MinX; x < MinX + W; x++)
            for (int y = MinY; y < MinY + H; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                bool border = x == MinX || x == MinX + W - 1 || y == MinY || y == MinY + H - 1;
                bool pillar = Pillars.Any(r => r.Contains(new Vector2Int(x, y)));
                if (border) walls.SetTile(cell, wallTile);
                else floor.SetTile(cell, floorTile);
                if (pillar) obstacles.SetTile(cell, wallTile);
            }

            // Satu objek per pilar: BoxCollider2D + ShadowCaster2D (di editor, caster memakai bentuk collider).
            foreach (var r in Pillars)
            {
                var p = new GameObject("PillarBody");
                p.layer = LayerMask.NameToLayer("Walls");
                p.transform.position = new Vector3(r.x + r.width * 0.5f, r.y + r.height * 0.5f, 0f);
                p.AddComponent<BoxCollider2D>().size = new Vector2(r.width, r.height);
                p.AddComponent<ShadowCaster2D>();
            }
        }

        static Tilemap MakeSolidTilemap(Grid grid, string name)
        {
            var tm = MakeTilemap(grid, name, 0);
            tm.gameObject.layer = LayerMask.NameToLayer("Walls");
            tm.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            tm.gameObject.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
            tm.gameObject.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;
            return tm;
        }

        static Tilemap MakeTilemap(Grid grid, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            return tm;
        }

        static Tile LoadOrCreateTile(string name, string sprite)
        {
            Directory.CreateDirectory(TilesDir);
            var path = TilesDir + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile != null) return tile;
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = LoadSprite(sprite);
            AssetDatabase.CreateAsset(tile, path);
            return tile;
        }

        public static Light2D MakePointLight(string name, Transform parent, string hex, float radius)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = Hex(hex);
            light.intensity = 1f;
            light.pointLightInnerRadius = 0f;
            light.shadowsEnabled = true;
            light.shadowIntensity = 0.9f;
            var src = go.AddComponent<LightSourceBehaviour>();
            var so = new SerializedObject(src);
            so.FindProperty("light2D").objectReferenceValue = light;
            so.FindProperty("radius").floatValue = radius;
            so.ApplyModifiedPropertiesWithoutUndo();
            light.pointLightOuterRadius = radius;
            return light;
        }

        static GameObject MakeSkeletonDummy(Vector3 pos)
        {
            var go = new GameObject("SkeletonDummy");
            go.transform.position = pos;
            go.AddComponent<SpriteRenderer>().sprite = LoadSprite("skeleton_0.png");
            AddEyes(go.transform, "skeleton_eyes_0.png");
            return go;
        }

        public static SpriteRenderer AddEyes(Transform parent, string sprite)
        {
            var eyes = new GameObject("Eyes");
            eyes.transform.SetParent(parent, false);
            var sr = eyes.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(sprite);
            sr.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(UnlitMat);
            sr.sortingOrder = 1;
            return sr;
        }

        public static Sprite LoadSprite(string file) => AssetDatabase.LoadAssetAtPath<Sprite>(Art + file);

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
    }
}
