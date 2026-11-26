using System;
using System.Collections.Generic;

namespace Runecrypt.Core
{
    // Semua keacakan gameplay dalam satu run berasal dari sini. Setiap sistem memakai stream bernama sendiri
    // ("floor", "spawn", "relic", "loot", "chest", ...), sehingga seed yang sama memberi undian ke-n yang sama di
    // setiap sistem, dan undian tambahan di satu sistem tidak menggeser sistem lain.
    // Kosmetik (guncangan, serpihan, pitch SFX) tidak memakai ini.
    public sealed class RunRandom
    {
        readonly RunSeed seed;
        readonly Dictionary<string, Random> streams = new Dictionary<string, Random>();

        public RunRandom(RunSeed seed) => this.seed = seed;

        public RunSeed Seed => seed;

        public Random Stream(string name)
        {
            if (!streams.TryGetValue(name, out var r))
            {
                r = new Random(StreamSeed(seed, name));
                streams[name] = r;
            }
            return r;
        }

        // FNV-1a 32-bit atas 4 byte seed (little-endian) lalu nama dalam UTF-16LE. Dibuat sendiri karena
        // string.GetHashCode tidak dijamin sama di setiap runtime.
        public static int StreamSeed(RunSeed seed, string name)
        {
            const uint Prime = 16777619u;
            uint h = 2166136261u;
            uint v = seed.Value;
            unchecked
            {
                for (int i = 0; i < 4; i++) { h = (h ^ (v & 0xFFu)) * Prime; v >>= 8; }
                foreach (char c in name)
                {
                    h = (h ^ (uint)(c & 0xFF)) * Prime;
                    h = (h ^ (uint)(c >> 8)) * Prime;
                }
                return (int)h;
            }
        }
    }
}
