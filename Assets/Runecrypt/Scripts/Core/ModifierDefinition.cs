using UnityEngine;

namespace Runecrypt.Core
{
    // Relic dan upgrade sama-sama daftar StatMod ditambah pemulihan HP. Konten baru = aset baru, bukan kode.
    public abstract class ModifierDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public StatMod[] mods;
        public bool healFull;
        public float heal;
    }
}
