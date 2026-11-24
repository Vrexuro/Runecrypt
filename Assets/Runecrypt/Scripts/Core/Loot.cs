namespace Runecrypt.Core
{
    // Angka acak masuk dari luar supaya undian bisa diuji.
    public static class Loot
    {
        public static bool Roll(float chance, float roll01) => roll01 < chance;
    }
}
