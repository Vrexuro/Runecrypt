using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.EditorTools3D
{
    // Runecrypt 3D (Docs/plans/2026-11-20-3d-playable.md): pengaturan proyek dan arena 3D.
    // Hanya ada di branch feature/3d-playable; versi 2D di folder utama tidak memakai builder ini.
    public static partial class Arena3DBuilder
    {
        const string UrpAssetPath = "Assets/Settings/UniversalRP.asset";
        const string Renderer3DPath = "Assets/Spike3D/Spike3DRenderer.asset";

        // Task 1: Universal Renderer (Forward+) jadi default, editor mode 3D, matriks tabrakan fisika 3D.
        [MenuItem("Runecrypt/3D/Configure Project")]
        public static void ConfigureProject()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Renderer3DPath);
            // Forward+: tidak ada batas 8 lampu per objek, jadi banyak bola api bercahaya tetap menerangi.
            data.renderingMode = RenderingMode.ForwardPlus;
            EditorUtility.SetDirty(data);

            var so = new SerializedObject(urp);
            var list = so.FindProperty("m_RendererDataList");
            int index = -1;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == data) index = i;
            if (index < 0) throw new System.InvalidOperationException("Spike3DRenderer belum ada di daftar renderer URP");
            so.FindProperty("m_DefaultRendererIndex").intValue = index;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode3D;

            int player = Layer("Player"), enemies = Layer("Enemies"), proj = Layer("Projectiles"), react = Layer("Reactives"), walls = Layer("Walls");
            Physics.IgnoreLayerCollision(proj, player, true);
            Physics.IgnoreLayerCollision(proj, proj, true);
            Physics.IgnoreLayerCollision(react, player, true);
            Physics.IgnoreLayerCollision(react, enemies, true);
            Physics.IgnoreLayerCollision(react, walls, true);
            Physics.IgnoreLayerCollision(react, react, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Arena3DBuilder] Proyek dikonfigurasi untuk 3D: renderer default index {index} (Forward+), mode 3D.");
        }

        static int Layer(string name)
        {
            int l = LayerMask.NameToLayer(name);
            if (l < 0) throw new System.InvalidOperationException("Layer tidak ada: " + name);
            return l;
        }
    }
}
