using NUnit.Framework;
using Runecrypt.Core;

namespace Runecrypt.Tests
{
    // Model stat M2: relic menumpuk lewat daftar StatMod (GDD §5.2, "relic bertumpuk").
    public class StatTests
    {
        static StatMod Mod(Stat s, StatOp op, float v) => new StatMod { stat = s, op = op, value = v };

        static ShotStats BaseShot() => new ShotStats
        {
            Damage = 3f, Speed = 12f, Range = 7f, Size = 1f, LightScale = 1f,
            BurnDuration = 2f, BurnDamagePerTick = 1f, Count = 1,
        };

        [Test]
        public void NoMods_ReturnsBase() => Assert.AreEqual(3f, new PlayerStats().Get(Stat.Damage, 3f));

        [Test]
        public void AddsThenPercent()
        {
            var s = new PlayerStats();
            s.Add(new[] { Mod(Stat.Damage, StatOp.Add, 1f), Mod(Stat.Damage, StatOp.Percent, 0.2f) });
            Assert.AreEqual(4.8f, s.Get(Stat.Damage, 3f), 1e-4f);
        }

        [Test]
        public void TwoSplitRelics_Stack()
        {
            var s = new PlayerStats();
            s.Add(new[] { Mod(Stat.ProjectileCount, StatOp.Add, 1f) });
            s.Add(new[] { Mod(Stat.ProjectileCount, StatOp.Add, 1f) });
            Assert.AreEqual(3, s.GetInt(Stat.ProjectileCount, 1));
        }

        [Test]
        public void NegativePercent_ClampedAtFloor()
        {
            var s = new PlayerStats();
            s.Add(new[] { Mod(Stat.ProjectileSpeed, StatOp.Percent, -1.5f) });
            Assert.AreEqual(1f, s.Shot(BaseShot()).Speed);
        }

        [Test]
        public void Shot_AppliesArangBerat()
        {
            var s = new PlayerStats();
            s.Add(new[]
            {
                Mod(Stat.ProjectileSize, StatOp.Percent, 0.4f),
                Mod(Stat.Damage, StatOp.Percent, 0.2f),
                Mod(Stat.ProjectileSpeed, StatOp.Percent, -0.15f),
            });
            var shot = s.Shot(BaseShot());
            Assert.AreEqual(3.6f, shot.Damage, 1e-4f);
            Assert.AreEqual(10.2f, shot.Speed, 1e-4f);
            Assert.AreEqual(1.4f, shot.Size, 1e-4f);
        }
    }
}
