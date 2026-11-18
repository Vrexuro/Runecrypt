using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    public class HealthTests
    {
        [Test]
        public void TakeDamage_ReducesCurrent()
        {
            var h = new Health(6);
            h.TakeDamage(1);
            Assert.AreEqual(5f, h.Current);
        }

        [Test]
        public void TakeDamage_Overkill_ClampsAtZero()
        {
            var h = new Health(12);
            h.TakeDamage(20);
            Assert.AreEqual(0f, h.Current);
            Assert.IsTrue(h.IsDead);
        }

        [Test]
        public void TakeDamage_AfterDeath_IsIgnored()
        {
            var h = new Health(12);
            int died = 0;
            h.Died += () => died++;
            h.TakeDamage(20);
            h.TakeDamage(3);
            Assert.AreEqual(1, died);
            Assert.AreEqual(0f, h.Current);
        }

        [Test]
        public void TakeDamage_Negative_IsIgnored()
        {
            var h = new Health(6);
            h.TakeDamage(-2);
            Assert.AreEqual(6f, h.Current);
        }

        [Test]
        public void SetMax_Raise_KeepsCurrent()
        {
            var h = new Health(6);
            h.SetMax(8);
            Assert.AreEqual(6f, h.Current);
            h.Heal(8);
            Assert.AreEqual(8f, h.Current);
        }

        [Test]
        public void SetMax_Lower_ClampsCurrent()
        {
            var h = new Health(6);
            h.SetMax(4);
            Assert.AreEqual(4f, h.Current);
        }

        [Test]
        public void Heal_WhenDead_DoesNothing()
        {
            var h = new Health(6);
            h.TakeDamage(6);
            h.Heal(3);
            Assert.AreEqual(0f, h.Current);
        }
    }

    public class LightRulesTests
    {
        LightRules rules;
        [SetUp] public void SetUp() => rules = ScriptableObject.CreateInstance<LightRules>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(rules);

        [Test] public void ApplyDamage_Lit_Adds25Percent() => Assert.AreEqual(3.75f, rules.ApplyDamage(3, true), 1e-4);
        [Test] public void ApplyDamage_Dark_Unchanged() => Assert.AreEqual(3f, rules.ApplyDamage(3, false), 1e-4);
        [Test] public void SpeedMultiplier_Dark_Is120Percent() => Assert.AreEqual(1.2f, rules.SpeedMultiplier(false), 1e-4);
        [Test] public void SpeedMultiplier_Lit_Is100Percent() => Assert.AreEqual(1f, rules.SpeedMultiplier(true), 1e-4);
    }
}
