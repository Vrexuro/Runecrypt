using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    // M2.5 Isi Makam (Docs/specs/2026-11-24-isi-makam-design.md).
    public class CryptKitTests
    {
        static int L(string name) => LayerMask.NameToLayer(name);

        [Test]
        public void PropsLayer_Exists() => Assert.GreaterOrEqual(L("Props"), 0);

        // Anglo dan Guci padat bagi pemain dan musuh, dan bisa dikenai tembakan; uap tidak menyentuhnya.
        [Test]
        public void PropsLayer_CollidesWithActorsNotReactives()
        {
            int props = L("Props");
            Assert.GreaterOrEqual(props, 0);
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Player")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Enemies")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Projectiles")));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(props, L("Reactives")));
        }

        // Task 1: logika inti.
        [Test]
        public void Brazier_IgnitesOnlyOnFire()
        {
            var b = new BrazierState();
            Assert.IsFalse(b.TryIgnite(Element.Ice));
            Assert.IsFalse(b.TryIgnite(Element.Lightning));
            Assert.IsFalse(b.TryIgnite(Element.None));
            Assert.IsFalse(b.IsLit);
            Assert.IsTrue(b.TryIgnite(Element.Fire));
            Assert.IsTrue(b.IsLit);
        }

        [Test]
        public void Brazier_IgnitesOnce()
        {
            var b = new BrazierState();
            b.TryIgnite(Element.Fire);
            Assert.IsFalse(b.TryIgnite(Element.Fire));
            Assert.IsTrue(b.IsLit);
        }

        [Test]
        public void Brazier_ResetExtinguishes()
        {
            var b = new BrazierState();
            b.TryIgnite(Element.Fire);
            b.Reset();
            Assert.IsFalse(b.IsLit);
        }

        [Test]
        public void Breakable_BreaksExactlyAtZero()
        {
            var s = new BreakableState(3f);
            Assert.IsFalse(s.Hit(2f));
            Assert.IsFalse(s.IsBroken);
            Assert.IsTrue(s.Hit(1f));
            Assert.IsTrue(s.IsBroken);
            Assert.IsFalse(s.Hit(5f));
        }

        [Test]
        public void Breakable_ResetRestoresHp()
        {
            var s = new BreakableState(3f);
            s.Hit(3f);
            s.Reset();
            Assert.IsFalse(s.IsBroken);
            Assert.AreEqual(3f, s.Hp);
        }

        [Test]
        public void Loot_RollRespectsChance()
        {
            Assert.IsTrue(Loot.Roll(0.15f, 0.149f));
            Assert.IsFalse(Loot.Roll(0.15f, 0.15f));
            Assert.IsFalse(Loot.Roll(0f, 0f));
        }
    }
}
