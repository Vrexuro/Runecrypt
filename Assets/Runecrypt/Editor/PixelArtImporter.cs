using UnityEditor;

namespace Runecrypt.EditorTools
{
    // Semua PNG di Assets/Runecrypt/Art otomatis jadi sprite pixel art 16 PPU (GDD §7.1).
    public class PixelArtImporter : AssetPostprocessor
    {
        // Naikkan saat aturan impor berubah supaya Unity mengimpor ulang sprite lama (2: FullRect untuk SpriteOutline).
        public override uint GetVersion() => 2;

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
            // FullRect: garis tepi Tersingkap (SpriteOutline) menggambar di piksel kosong sekitar bentuk sprite.
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
        }
    }
}
