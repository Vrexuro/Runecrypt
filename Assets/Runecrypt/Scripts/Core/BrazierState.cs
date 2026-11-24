namespace Runecrypt.Core
{
    // Anglo (spec Isi Makam §5.2): menyala sekali oleh api, padam lagi saat ruangan dimulai ulang.
    public class BrazierState
    {
        public bool IsLit { get; private set; }

        // True hanya saat berubah dari padam ke menyala.
        public bool TryIgnite(Element e)
        {
            if (IsLit || e != Element.Fire) return false;
            IsLit = true;
            return true;
        }

        public void Reset() => IsLit = false;
    }
}
