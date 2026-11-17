using NUnit.Framework;
using Runecrypt.Core;

namespace Runecrypt.Tests
{
    public class CooldownTests
    {
        [Test] public void New_IsReady() => Assert.IsTrue(new Cooldown(0.8f).IsReady(0f));

        [Test]
        public void Start_MakesNotReadyImmediately()
        {
            var c = new Cooldown(0.8f);
            c.Start(1f);
            Assert.IsFalse(c.IsReady(1f));
        }

        [Test]
        public void IsReady_AfterSeconds()
        {
            var c = new Cooldown(0.8f);
            c.Start(1f);
            Assert.IsFalse(c.IsReady(1.79f));
            Assert.IsTrue(c.IsReady(1.8f));
        }

        [Test]
        public void Remaining()
        {
            var c = new Cooldown(0.8f);
            c.Start(1f);
            Assert.AreEqual(0.3f, c.Remaining(1.5f), 1e-4);
            Assert.AreEqual(0f, c.Remaining(5f));
        }
    }

    public class BurnStatusTests
    {
        [Test]
        public void Tick_PaysOncePerSecond()
        {
            var b = new BurnStatus();
            b.Apply(0f);
            Assert.AreEqual(0f, b.Tick(0.9f));
            Assert.AreEqual(1f, b.Tick(1f));
        }

        [Test]
        public void Tick_StopsAtDuration()
        {
            var b = new BurnStatus();
            b.Apply(0f);
            b.Tick(1f);
            Assert.AreEqual(1f, b.Tick(2.5f));
            Assert.AreEqual(0f, b.Tick(5f));
        }

        [Test]
        public void IsActive_EndsAfterDuration()
        {
            var b = new BurnStatus();
            b.Apply(0f);
            Assert.IsTrue(b.IsActive(1.9f));
            Assert.IsFalse(b.IsActive(2.01f));
        }

        [Test]
        public void Reapply_RefreshesDurationWithoutStackingTicks()
        {
            var b = new BurnStatus();
            b.Apply(0f); b.Apply(0.33f); b.Apply(0.66f); b.Apply(1.5f);
            Assert.AreEqual(3f, b.Tick(4f)); // ticks at 1, 2, 3; end 3.5
        }

        [Test]
        public void Apply_AfterExpiry_StartsFreshSchedule()
        {
            var b = new BurnStatus();
            b.Apply(0f);
            b.Tick(5f);
            b.Apply(10f);
            Assert.AreEqual(0f, b.Tick(10.5f));
            Assert.AreEqual(1f, b.Tick(11f));
        }
    }
}
