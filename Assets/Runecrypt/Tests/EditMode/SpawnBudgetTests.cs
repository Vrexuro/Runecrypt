using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;

namespace Runecrypt.Tests
{
    // M3a Task 5: gelombang campuran disusun dari budget (budget = WaveSchedule.NextWaveSize()).
    public class SpawnBudgetTests
    {
        static readonly int[] Costs = { 1, 2, 3 };

        [Test]
        public void Compose_NeverExceedsBudget()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var picks = SpawnBudget.Compose(7, Costs, new System.Random(seed), 8);
                Assert.LessOrEqual(picks.Sum(i => Costs[i]), 7, $"seed {seed}");
                Assert.IsNotEmpty(picks, $"seed {seed}");
            }
        }

        [Test]
        public void Compose_NeverExceedsCap() =>
            Assert.AreEqual(8, SpawnBudget.Compose(100, new[] { 1 }, new System.Random(1), 8).Count);

        // Review Focus 3: budget yang tidak habis dibagi tidak membuat penyusunan berputar tanpa akhir.
        [Test]
        public void Compose_StopsWhenNothingAffordable() =>
            Assert.IsEmpty(SpawnBudget.Compose(1, new[] { 2, 3 }, new System.Random(1), 8));

        [Test]
        public void Compose_ZeroCostCountsAsOne() =>
            Assert.AreEqual(3, SpawnBudget.Compose(3, new[] { 0 }, new System.Random(1), 8).Count);

        [Test]
        public void Compose_SameSeedSameWave() =>
            CollectionAssert.AreEqual(SpawnBudget.Compose(9, Costs, new System.Random(42), 8),
                SpawnBudget.Compose(9, Costs, new System.Random(42), 8));
    }
}
