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

        // Task 1: Universal Renderer (Forward+) untuk kamera 3D dan matriks tabrakan fisika 3D.
        // Renderer default dan mode editor sengaja TIDAK diubah (review akhir, Important 4): scene 2D di branch ini
        // dan hasil merge ke main tetap memakai Renderer2D. Kamera Arena3D memilih renderer 3D sendiri.
        [MenuItem("Runecrypt/3D/Configure Project")]
        public static void ConfigureProject()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Renderer3DPath);
            // Forward+: tidak ada batas 8 lampu per objek, jadi banyak bola api bercahaya tetap menerangi.
            data.renderingMode = RenderingMode.ForwardPlus;
            EditorUtility.SetDirty(data);

            int index = Renderer3DIndex();
            var so = new SerializedObject(urp);
            so.FindProperty("m_DefaultRendererIndex").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;

            int player = Layer("Player"), enemies = Layer("Enemies"), proj = Layer("Projectiles"), react = Layer("Reactives"), walls = Layer("Walls");
            Physics.IgnoreLayerCollision(proj, player, true);
            Physics.IgnoreLayerCollision(proj, proj, true);
            Physics.IgnoreLayerCollision(react, player, true);
            Physics.IgnoreLayerCollision(react, enemies, true);
            Physics.IgnoreLayerCollision(react, walls, true);
            Physics.IgnoreLayerCollision(react, react, true);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Arena3DBuilder] Renderer 3D index {index} (Forward+); default tetap Renderer2D.");
        }

        static int Renderer3DIndex()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Renderer3DPath);
            var list = new SerializedObject(urp).FindProperty("m_RendererDataList");
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == data) return i;
            throw new System.InvalidOperationException("Spike3DRenderer belum ada di daftar renderer URP");
        }

        static int Layer(string name)
        {
            int l = LayerMask.NameToLayer(name);
            if (l < 0) throw new System.InvalidOperationException("Layer tidak ada: " + name);
            return l;
        }
    }
}
