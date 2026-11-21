using System.Collections.Generic;
using System.IO;
using System.Linq;
using Runecrypt.Core;
using Runecrypt.Game3D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.EditorTools3D
{
    // Task 6: arena 3D seukuran ruangan 2D (M1Builder: 20×11 tile, pilar sama), prefab, cahaya, bloom, dan loop.
    public static partial class Arena3DBuilder
    {
        const string Root = "Assets/Runecrypt3D/";
        const string ScenePath = Root + "Scenes/Arena3D.unity";
        const string PyroModel = Root + "Models/Pyromancer3D.fbx";
        const string TulangModel = "Assets/Spike3D/Models/TulangPenjaga3D.fbx";

        static readonly Color Ember = new Color(1f, 0.55f, 0.25f);

        [MenuItem("Runecrypt/3D/Build Arena")]
        public static void BuildArena()
        {
            ConfigureProject();
            EnableAdditionalLightShadows();
            foreach (var d in new[] { "Scenes", "Prefabs", "Materials" }) Directory.CreateDirectory(Root + d);
            ConfigureRig(TulangModel);
            ConfigureRig(PyroModel);
            RemapPyromancerMaterials();
            FixTulangEye();

            BuildPlayer(BuildFireball());
            BuildTulang();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.07f);   // makam yang menelan cahaya
            BuildRoom();
            // NewScene melepas aset yang sudah dimuat, jadi prefab dimuat ulang dari path-nya
            var p = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Player3D.prefab"));
            p.transform.position = new Vector3(0f, 0f, -0.5f);
            BuildCamera(p.transform);
            BuildVolume();
            BuildDirector(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Tulang3D.prefab").GetComponent<Enemy3D>());

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
            Debug.Log("[Arena3DBuilder] Arena3D dibangun.");
        }

        // ---------- ruangan ----------
        static void BuildRoom()
        {
            var floorMat = Lit("Arena_Floor", new Color(0.14f, 0.13f, 0.16f), smooth: 0.1f);
            var wallMat = Lit("Arena_Wall", new Color(0.2f, 0.18f, 0.22f), smooth: 0.05f);
            var room = new GameObject("Room").transform;
            Block("Floor", new Vector3(0f, -0.05f, -0.5f), new Vector3(20f, 0.1f, 11f), floorMat, room, "Default");
            // dinding pinggir di sel tepi (x -10..9, y -6..4 di M1Builder), tinggi 1,5
            Block("WallNorth", new Vector3(0f, 0.75f, 4.5f), new Vector3(20f, 1.5f, 1f), wallMat, room, "Walls");
            Block("WallSouth", new Vector3(0f, 0.75f, -5.5f), new Vector3(20f, 1.5f, 1f), wallMat, room, "Walls");
            Block("WallWest", new Vector3(-9.5f, 0.75f, -0.5f), new Vector3(1f, 1.5f, 11f), wallMat, room, "Walls");
            Block("WallEast", new Vector3(9.5f, 0.75f, -0.5f), new Vector3(1f, 1.5f, 11f), wallMat, room, "Walls");
            // pilar sama dengan M1Builder.Pillars: RectInt(-5,-2,2,2) dan RectInt(3,0,2,2)
            Block("Pillar0", new Vector3(-4f, 0.75f, -1f), new Vector3(2f, 1.5f, 2f), wallMat, room, "Walls");
            Block("Pillar1", new Vector3(4f, 0.75f, 1f), new Vector3(2f, 1.5f, 2f), wallMat, room, "Walls");
        }

        static void Block(string name, Vector3 pos, Vector3 size, Material mat, Transform parent, string layer)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.layer = LayerMask.NameToLayer(layer);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // ---------- prefab ----------
        static Fireball3D BuildFireball()
        {
            var go = new GameObject("Fireball3D");
            go.layer = LayerMask.NameToLayer("Projectiles");
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(core.GetComponent<Collider>());
            core.transform.SetParent(go.transform, false);
            core.transform.localScale = Vector3.one * 0.22f;
            core.GetComponent<Renderer>().sharedMaterial = Lit("Fireball_Core", new Color(1f, 0.45f, 0.1f), new Color(6f, 2.2f, 0.5f));
            var light = PointLight(go.transform, "Light", 1.5f, 4f, false);
            var src = go.AddComponent<LightSource3D>();
            SetRef(src, "visual", light);
            SetFloat(src, "radius", 1.5f);
            go.AddComponent<Fireball3D>();
            return Save(go, "Fireball3D").GetComponent<Fireball3D>();
        }

        static GameObject BuildPlayer(Fireball3D fireball)
        {
            var go = new GameObject("Player3D");
            go.layer = LayerMask.NameToLayer("Player");
            Capsule(go);
            go.AddComponent<Rigidbody>();
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            // lampu pemain ada di dekat tubuhnya sendiri; tanpa ini tubuh pemain melempar bayangan raksasa ke depan
            foreach (var r in Model(PyroModel, visual).GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
            // cahaya pemain GDD §4.2: radius 3 tile, jingga hangat, dengan bayangan supaya pilar memotong cahaya
            var light = PointLight(go.transform, "PlayerLight", 3f, 7f, true);
            // lampu sedikit di depan tapi tetap di dalam kapsul (|z| <= 0,25), supaya tidak masuk ke pilar saat menempel
            light.transform.localPosition = new Vector3(0f, 0.95f, -0.22f);
            // titik aturan cahaya di pusat pemain (root), bukan di posisi lampu
            var src = go.AddComponent<LightSource3D>();
            SetRef(src, "visual", light);
            SetFloat(src, "radius", 3f);
            var p = go.AddComponent<Player3D>();
            SetRef(p, "fireballPrefab", fireball);
            SetRef(p, "visual", visual);
            return Save(go, "Player3D");
        }

        static Enemy3D BuildTulang()
        {
            var go = new GameObject("Tulang3D");
            go.layer = LayerMask.NameToLayer("Enemies");
            Capsule(go);
            go.AddComponent<Rigidbody>();
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);
            var model = Model(TulangModel, visual);
            var e = go.AddComponent<Enemy3D>();
            SetRef(e, "definition", AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/Runecrypt/Data/Enemies/TulangPenjaga.asset"));
            SetRef(e, "lightRules", AssetDatabase.LoadAssetAtPath<LightRules>("Assets/Runecrypt/Data/LightRules.asset"));
            var so = new SerializedObject(e);
            var arr = so.FindProperty("flashRenderers");
            var rs = model.GetComponentsInChildren<Renderer>();
            arr.arraySize = rs.Length;
            for (int i = 0; i < rs.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = rs[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return Save(go, "Tulang3D").GetComponent<Enemy3D>();
        }

        static void Capsule(GameObject go)
        {
            var c = go.AddComponent<CapsuleCollider>();
            c.center = new Vector3(0f, 0.5f, 0f); c.radius = 0.3f; c.height = 1f;
        }

        static GameObject Model(string path, Transform parent)
        {
            var m = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            m.transform.SetParent(parent, false);
            // depan model FBX sudah +Z; putaran 180° di Spike3DSetup hanya untuk memamerkan wajah ke kamera
            foreach (var r in m.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.On;
            return m;
        }

        static Light PointLight(Transform parent, string name, float radius, float intensity, bool shadows)
        {
            var l = new GameObject(name).AddComponent<Light>();
            l.transform.SetParent(parent, false);
            l.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            l.type = LightType.Point;
            l.color = Ember;
            l.range = radius * 1.4f;
            l.intensity = intensity;
            l.shadows = shadows ? LightShadows.Soft : LightShadows.None;
            return l;
        }

        // ---------- kamera, bloom, director ----------
        static void BuildCamera(Transform player)
        {
            var cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 3.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.transform.rotation = Quaternion.Euler(42f, 0f, 0f);
            cam.transform.position = player.position - cam.transform.forward * 20f;
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 60f;
            var data = cam.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.SetRenderer(Renderer3DIndex());   // default URP tetap Renderer2D untuk scene 2D
            var follow = cam.gameObject.AddComponent<CameraFollow3D>();
            SetRef(follow, "target", player);
        }

        static void BuildVolume()
        {
            const string path = Root + "Arena3DVolume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }
            if (!profile.TryGet<Bloom>(out var bloom)) bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.9f);
            bloom.intensity.Override(1.3f);
            bloom.scatter.Override(0.6f);
            if (!profile.TryGet<Vignette>(out var vig)) vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.35f);
            EditorUtility.SetDirty(profile);
            var v = new GameObject("GlobalVolume").AddComponent<Volume>();
            v.isGlobal = true;
            v.sharedProfile = profile;
        }

        static void BuildDirector(Enemy3D tulang)
        {
            var go = new GameObject("Arena3DDirector");
            var d = go.AddComponent<Arena3DDirector>();
            // titik spawn sama dengan arena 2D (M1ActorBuilder), (x, y) -> (x, 0, y)
            Vector2[] pts = { new Vector2(-8f, 3f), new Vector2(8f, 3f), new Vector2(-8f, -4.5f), new Vector2(8f, -4.5f), new Vector2(-0.5f, 3.2f), new Vector2(-0.5f, -4.5f) };
            var so = new SerializedObject(d);
            so.FindProperty("enemyPrefab").objectReferenceValue = tulang;
            var arr = so.FindProperty("spawnPoints");
            arr.arraySize = pts.Length;
            for (int i = 0; i < pts.Length; i++)
            {
                var sp = new GameObject($"Spawn{i}").transform;
                sp.SetParent(go.transform, false);
                sp.position = new Vector3(pts[i].x, 0f, pts[i].y);
                arr.GetArrayElementAtIndex(i).objectReferenceValue = sp;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- utilitas ----------
        // Rig dari script Blender (m3d_*_rig.py): Generic, klip Idle/Move berulang, tanpa kompresi (rig kecil mudah bergetar).
        static void ConfigureRig(string modelPath)
        {
            var imp = (ModelImporter)AssetImporter.GetAtPath(modelPath);
            imp.animationType = ModelImporterAnimationType.Generic;
            imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            imp.animationCompression = ModelImporterAnimationCompression.Off;
            imp.SaveAndReimport();   // take baru terbaca sebagai defaultClipAnimations setelah tipe Generic
            var clips = imp.defaultClipAnimations;
            foreach (var c in clips)
            {
                c.name = c.name.Substring(c.name.LastIndexOf('|') + 1);   // "Rig|Move" -> "Move"
                c.loopTime = true;
            }
            imp.clipAnimations = clips;
            imp.SaveAndReimport();
        }

        static void RemapPyromancerMaterials()
        {
            var mats = new Dictionary<string, Material>
            {
                { "Pyro_Robe", Lit("Pyro_Robe", Hex(40, 31, 56), smooth: 0.05f) },
                { "Pyro_RobeDark", Lit("Pyro_RobeDark", Hex(22, 17, 31), smooth: 0.05f) },
                { "Pyro_Void", Lit("Pyro_Void", Hex(3, 2, 5), smooth: 0f) },
                { "Pyro_Eye", Lit("Pyro_Eye", Hex(255, 100, 20), new Color(5f, 1.6f, 0.3f)) },
                { "Pyro_Ember", Lit("Pyro_Ember", Hex(255, 90, 20), new Color(7f, 2.2f, 0.4f)) },
                { "Pyro_Burn", Lit("Pyro_Burn", Hex(255, 70, 15), new Color(4f, 1.1f, 0.2f)) },
                { "Pyro_Hand", Lit("Pyro_Hand", Hex(44, 36, 38)) },
                { "Pyro_Leather", Lit("Pyro_Leather", Hex(58, 38, 28)) },
                { "Pyro_Bone", Lit("Pyro_Bone", Hex(196, 186, 164)) },
            };
            var importer = (ModelImporter)AssetImporter.GetAtPath(PyroModel);
            foreach (var kv in mats) importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), kv.Key), kv.Value);
            importer.SaveAndReimport();
        }

        static void EnableAdditionalLightShadows()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            var so = new SerializedObject(urp);
            so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AddToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));   // di akhir: scene 2D tetap index 0
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static Color Hex(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f);

        static Material Lit(string name, Color baseColor, Color? emission = null, float smooth = 0.2f)
        {
            string path = Root + "Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smooth);
            if (emission.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission.Value);
                // URP 17 mematikan _EMISSION saat validasi material kalau flag GI tidak emissive
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Material mata Tulang dibuat Spike3DSetup; perbaiki flag GI-nya di sini supaya Build Arena saja cukup.
        static void FixTulangEye()
        {
            var eye = AssetDatabase.LoadAssetAtPath<Material>("Assets/Spike3D/Materials/Tulang_Eye.mat");
            eye.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            eye.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(eye);
        }

        static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Save(GameObject go, string name)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefabs/" + name + ".prefab");
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
