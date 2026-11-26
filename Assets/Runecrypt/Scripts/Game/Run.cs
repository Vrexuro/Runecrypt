using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Run yang sedang berjalan: seed-nya dan semua stream acak gameplay (lihat RunRandom).
    // ArenaDirector memulai run baru setiap scene dimuat (termasuk ulang dengan R); di M4 dipindah ke awal lantai.
    // Sistem gameplay mengambil acak lewat Run.Random.Stream("<sistem>"), bukan UnityEngine.Random.
    public static class Run
    {
        static RunRandom current;
        static RunSeed? forced;

        // Kalau belum ada run (misalnya benda diuji sendiri), run baru dengan seed acak dimulai.
        public static RunRandom Random => current ??= new RunRandom(RunSeed.New());
        public static RunSeed Seed => Random.Seed;

        public static void Begin(RunSeed seed) => current = new RunRandom(seed);

        // Seed yang dipaksa (input pemain kelak, debug, test) dipakai sekali, lalu kembali acak.
        public static void BeginNext()
        {
            Begin(forced ?? RunSeed.New());
            forced = null;
        }

        public static void ForceNextSeed(RunSeed seed) => forced = seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => current = null;

        internal static void ResetForTests()
        {
            current = null;
            forced = null;
        }
    }
}
