using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;

namespace Runecrypt.Tests
{
    public class RunSeedTests
    {
        [TestCase(0u)]
        [TestCase(1u)]
        [TestCase(123456789u)]
        [TestCase(uint.MaxValue)]
        public void Code_RoundTrips(uint value)
        {
            var seed = new RunSeed(value);
            string code = seed.ToString();
            Assert.IsTrue(RunSeed.TryParse(code, out var back), code);
            Assert.AreEqual(value, back.Value);
        }

        [Test]
        public void Code_IsEightCharsWithDash()
        {
            string code = new RunSeed(987654321u).ToString();
            StringAssert.IsMatch("^[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}$", code);
        }

        // Kelak pemain mengetik seed: huruf kecil, tanpa tanda hubung, dan O/I/L yang tertukar tetap diterima.
        [Test]
        public void Parse_IsForgivingAboutTyping()
        {
            var seed = new RunSeed(42u);
            string code = seed.ToString();
            string sloppy = code.Replace("-", " ").ToLowerInvariant().Replace('0', 'o').Replace('1', 'l');
            Assert.IsTrue(RunSeed.TryParse(sloppy, out var back), sloppy);
            Assert.AreEqual(seed, back);
        }

        // Karakter terakhir adalah checksum: satu huruf salah ketik ditolak, bukan diam-diam jadi run lain.
        [Test]
        public void Parse_RejectsTypoAndGarbage()
        {
            string code = new RunSeed(123456789u).ToString();
            char wrong = code[2] == 'A' ? 'B' : 'A';
            string typo = code.Substring(0, 2) + wrong + code.Substring(3);
            Assert.IsFalse(RunSeed.TryParse(typo, out _), typo);
            Assert.IsFalse(RunSeed.TryParse("", out _));
            Assert.IsFalse(RunSeed.TryParse("ABC", out _));
            Assert.IsFalse(RunSeed.TryParse("UUUU-UUUU", out _));
        }

        [Test]
        public void New_GivesDifferentSeeds()
        {
            var seeds = new HashSet<uint>(Enumerable.Range(0, 50).Select(_ => RunSeed.New().Value));
            Assert.Greater(seeds.Count, 45);
        }
    }

    public class RunRandomTests
    {
        static int[] Draw(System.Random r, int n = 20) => Enumerable.Range(0, n).Select(_ => r.Next(1000)).ToArray();

        [Test]
        public void SameSeedAndName_GiveSameSequence()
        {
            var a = new RunRandom(new RunSeed(777u));
            var b = new RunRandom(new RunSeed(777u));
            CollectionAssert.AreEqual(Draw(a.Stream("floor")), Draw(b.Stream("floor")));
        }

        [Test]
        public void DifferentSeeds_GiveDifferentSequences()
        {
            var a = new RunRandom(new RunSeed(777u));
            var b = new RunRandom(new RunSeed(778u));
            CollectionAssert.AreNotEqual(Draw(a.Stream("floor")), Draw(b.Stream("floor")));
        }

        [Test]
        public void DifferentNames_GiveDifferentSequences()
        {
            var r = new RunRandom(new RunSeed(777u));
            CollectionAssert.AreNotEqual(Draw(r.Stream("floor")), Draw(r.Stream("loot")));
        }

        // Inti fitur seed: undian di satu sistem tidak menggeser sistem lain.
        [Test]
        public void DrawingFromOneStream_DoesNotShiftAnother()
        {
            var clean = new RunRandom(new RunSeed(31337u));
            var busy = new RunRandom(new RunSeed(31337u));
            Draw(busy.Stream("loot"), 57);
            CollectionAssert.AreEqual(Draw(clean.Stream("floor")), Draw(busy.Stream("floor")));
        }

        [Test]
        public void Stream_IsCachedPerRun()
        {
            var r = new RunRandom(new RunSeed(5u));
            Assert.AreSame(r.Stream("relic"), r.Stream("relic"));
        }

        // Hash buatan sendiri (FNV-1a), bukan string.GetHashCode, supaya seed sama di setiap mesin dan build.
        [Test]
        public void StreamSeed_IsStable()
        {
            Assert.AreEqual(RunRandom.StreamSeed(new RunSeed(0u), "floor"), RunRandom.StreamSeed(new RunSeed(0u), "floor"));
            // Dihitung terpisah: FNV-1a 32-bit atas 4 byte seed (little-endian) lalu UTF-16LE nama.
            Assert.AreEqual(2064469837, RunRandom.StreamSeed(new RunSeed(0u), "floor"));
        }
    }
}
