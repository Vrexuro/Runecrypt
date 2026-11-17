namespace Runecrypt.Core
{
    // Jeda game milik ArenaDirector (layar Tumbang). Hit-stop tidak boleh mengembalikan timeScale selama ini aktif.
    public static class GamePause
    {
        public static bool IsPaused { get; set; }
    }
}
