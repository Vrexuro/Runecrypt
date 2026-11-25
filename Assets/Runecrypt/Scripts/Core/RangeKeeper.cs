namespace Runecrypt.Core
{
    // Musuh jarak jauh menjaga jarak nyaman (rencana M3a Task 4).
    public static class RangeKeeper
    {
        // -1 menjauh, 0 diam, +1 mendekat.
        public static float Direction(float distance, float min, float max) =>
            distance < min ? -1f : distance > max ? 1f : 0f;
    }
}
