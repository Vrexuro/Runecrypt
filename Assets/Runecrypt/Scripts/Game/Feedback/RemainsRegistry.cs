using System.Collections.Generic;
using UnityEngine;

namespace Runecrypt.Game
{
    // Sisa di lantai (tumpukan abu). Dibatasi supaya ruangan yang ramai tidak menumpuk objek tanpa akhir.
    public static class RemainsRegistry
    {
        public static int Capacity = 30;
        static readonly List<GameObject> items = new List<GameObject>();

        public static int Count => items.Count;

        public static void Add(GameObject go)
        {
            items.RemoveAll(g => g == null); // sudah hancur, misalnya scene dimuat ulang
            items.Add(go);
            while (items.Count > Capacity)
            {
                var oldest = items[0];
                items.RemoveAt(0);
                if (oldest == null) continue;
                if (Application.isPlaying) Object.Destroy(oldest);
                else Object.DestroyImmediate(oldest);
            }
        }

        // Hanya mengosongkan daftar; objeknya ikut hilang saat scene dimuat ulang.
        public static void Clear() => items.Clear();
    }
}
