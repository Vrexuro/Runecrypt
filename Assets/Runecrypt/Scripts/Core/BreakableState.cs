namespace Runecrypt.Core
{
    // Benda yang bisa dihancurkan, misalnya Guci Abu (spec Isi Makam §5.3).
    public class BreakableState
    {
        readonly float maxHp;

        public float Hp { get; private set; }
        public bool IsBroken => Hp <= 0f;

        public BreakableState(float maxHp)
        {
            this.maxHp = maxHp;
            Hp = maxHp;
        }

        // True hanya pada pukulan yang membuatnya pecah.
        public bool Hit(float damage)
        {
            if (IsBroken) return false;
            Hp -= damage;
            return IsBroken;
        }

        public void Reset() => Hp = maxHp;
    }
}
