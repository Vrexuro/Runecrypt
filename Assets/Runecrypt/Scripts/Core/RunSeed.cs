using System;

namespace Runecrypt.Core
{
    // Seed satu run (32 bit). Ditampilkan sebagai kode 8 karakter "XXXX-XXXX" berhuruf Crockford base32
    // (tanpa I, L, O, U supaya tidak tertukar): 7 karakter data + 1 checksum, sehingga seed yang diketik pemain
    // dengan satu huruf salah ditolak, bukan diam-diam menjadi run lain.
    public readonly struct RunSeed : IEquatable<RunSeed>
    {
        const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
        const int DataDigits = 7;

        public readonly uint Value;

        public RunSeed(uint value) => Value = value;

        public static RunSeed New() => new RunSeed(BitConverter.ToUInt32(Guid.NewGuid().ToByteArray(), 0));

        public override string ToString()
        {
            var digits = new int[DataDigits];
            uint v = Value;
            for (int i = DataDigits - 1; i >= 0; i--) { digits[i] = (int)(v & 31); v >>= 5; }
            var chars = new char[9];
            int c = 0;
            for (int i = 0; i < DataDigits; i++)
            {
                if (i == 4) chars[c++] = '-';
                chars[c++] = Alphabet[digits[i]];
            }
            chars[c] = Alphabet[Checksum(digits)];
            return new string(chars);
        }

        // Longgar terhadap cara mengetik: huruf kecil, spasi, tanda hubung, dan O/I/L dibaca sebagai 0/1/1.
        public static bool TryParse(string text, out RunSeed seed)
        {
            seed = default;
            if (string.IsNullOrEmpty(text)) return false;
            var digits = new int[DataDigits + 1];
            int n = 0;
            foreach (char raw in text)
            {
                if (raw == '-' || char.IsWhiteSpace(raw)) continue;
                char ch = char.ToUpperInvariant(raw);
                if (ch == 'O') ch = '0';
                else if (ch == 'I' || ch == 'L') ch = '1';
                int d = Alphabet.IndexOf(ch);
                if (d < 0 || n == digits.Length) return false;
                digits[n++] = d;
            }
            if (n != digits.Length) return false;

            ulong value = 0;
            for (int i = 0; i < DataDigits; i++) value = (value << 5) | (uint)digits[i];
            if (value > uint.MaxValue) return false;
            if (digits[DataDigits] != Checksum(digits)) return false;
            seed = new RunSeed((uint)value);
            return true;
        }

        // Bobot ganjil (1, 3, 5, ...) bisa dibalik modulo 32, jadi setiap salah ketik satu karakter terdeteksi.
        static int Checksum(int[] digits)
        {
            int sum = 0;
            for (int i = 0; i < DataDigits; i++) sum += (2 * i + 1) * digits[i];
            return sum & 31;
        }

        public bool Equals(RunSeed other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RunSeed s && Equals(s);
        public override int GetHashCode() => (int)Value;
    }
}
