using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Runecrypt.EditorTools
{
    // Builder membuat GameObject baru setiap Build All. Komponen dengan [SerializeReference] (misalnya
    // ShadowCaster2D) lalu diberi managed reference id (rid) baru, sehingga prefab tampil berubah di git
    // walaupun desainnya sama.
    //
    // Unity tidak mengizinkan rid diganti setelah ditetapkan, dan rid sudah ditetapkan begitu objek
    // diserialisasi (termasuk saat SerializedObject dibuat). Karena itu rid ditulis ulang langsung di file
    // YAML setelah disimpan: setiap rid positif diganti, menurut urutan kemunculan, dengan id yang diturunkan
    // dari path asset. rid negatif (-1 unknown, -2 null) dibiarkan.
    public static class StableRefIds
    {
        static readonly Regex Rid = new Regex(@"(?<=\brid: )\d+", RegexOptions.Compiled);

        // Mengembalikan true kalau file berubah (lalu di-reimport).
        public static bool RewriteFile(string assetPath)
        {
            var text = File.ReadAllText(assetPath);
            var map = new Dictionary<string, string>();
            var result = Rid.Replace(text, m =>
            {
                if (!map.TryGetValue(m.Value, out var stable))
                {
                    stable = StableId(assetPath + "#" + map.Count).ToString();
                    map[m.Value] = stable;
                }
                return stable;
            });
            if (result == text) return false;
            File.WriteAllText(assetPath, result);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        // FNV-1a 64-bit, dibuat positif (rid negatif dipakai Unity untuk null/unknown).
        public static long StableId(string key)
        {
            ulong hash = 14695981039346656037UL;
            foreach (char c in key)
            {
                hash ^= c;
                hash *= 1099511628211UL;
            }
            long id = (long)(hash & long.MaxValue);
            return id == 0 ? 1 : id;
        }
    }
}
