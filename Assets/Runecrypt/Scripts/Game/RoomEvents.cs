using System;

namespace Runecrypt.Game
{
    // "Ruangan dimulai": benda makam kembali ke keadaan awal (spec Isi Makam §6).
    // Di arena M1/M2 dipicu setiap gelombang baru; di M4 dipicu saat pemain masuk ruangan.
    public static class RoomEvents
    {
        public static event Action Started;

        public static void RaiseStarted() => Started?.Invoke();

        internal static void ClearForTests() => Started = null;
    }
}
