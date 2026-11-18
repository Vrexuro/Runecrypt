using UnityEditor;

namespace Runecrypt.EditorTools
{
    // Semua PNG di Assets/Runecrypt/Art otomatis jadi sprite pixel art 16 PPU (GDD §7.1).
    public class PixelArtImporter : AssetPostprocessor
    {
        // Naikkan saat aturan impor berubah supaya Unity mengimpor ulang sprite lama (3: kembali ke mesh Tight setelah garis tepi Tersingkap dihapus).
        public override uint GetVersion() => 3;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Runecrypt/Art/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16;
            ti.filterMode = UnityEngine.FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = UnityEngine.SpriteMeshType.Tight;
            ti.SetTextureSettings(settings);
        }
    }
}
