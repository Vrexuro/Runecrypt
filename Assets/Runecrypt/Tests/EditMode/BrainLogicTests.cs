using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    // M3a: logika murni otak musuh (siklus menukik Pelita Sesat, penjaga jarak Biarawan Beku).
    public class BrainLogicTests
    {
        // Hover 1,4 + Windup 0,35 + Dive 0,45 + Retreat 0,6 (keputusan Claude, rencana M3a Task 3).
        static SwoopCycle Cycle()
        {
            var s = new SwoopCycle(1.4f, 0.35f, 0.45f, 0.6f, 3f, 0.8f);
            s.Reset(0f);
            return s;
        }

        [Test]
        public void Swoop_HoversThenDivesAtLockedDirection()
        {
            var s = Cycle();
            Assert.AreEqual(Vector2.zero, s.Tick(0.5f, Vector2.right));
            Assert.AreEqual(SwoopPhase.Hover, s.Phase);
            Assert.AreEqual(Vector2.zero, s.Tick(1.5f, Vector2.right * 5f), "ancang-ancang: diam");
            Assert.AreEqual(SwoopPhase.Windup, s.Phase);
            s.Tick(1.7f, Vector2.up * 5f); // pemain pindah selama ancang-ancang
            Vector2 dive = s.Tick(1.8f, Vector2.up * 5f);
            Assert.AreEqual(SwoopPhase.Dive, s.Phase);
            Assert.AreEqual(3f, dive.x, 1e-4f, "arah dikunci di awal ancang-ancang");
            Assert.AreEqual(0f, dive.y, 1e-4f);
        }

        [Test]
        public void Swoop_DiveSpeedIsTripled()
        {
            var s = Cycle();
            s.Tick(1.5f, new Vector2(3f, 4f));
            Assert.AreEqual(3f, s.Tick(1.8f, new Vector2(3f, 4f)).magnitude, 1e-4f);
        }

        [Test]
        public void Swoop_RetreatsAwayFromPlayer()
        {
            var s = Cycle();
            s.Tick(1.5f, Vector2.right);
            s.Tick(1.8f, Vector2.right);
            Vector2 back = s.Tick(2.3f, Vector2.right * 2f);
            Assert.AreEqual(SwoopPhase.Retreat, s.Phase);
            Assert.Less(back.x, 0f, "menjauhi pemain");
            Assert.AreEqual(0.8f, back.magnitude, 1e-4f);
        }

        [Test]
        public void Swoop_ResetReturnsToHover()
        {
            var s = Cycle();
            s.Tick(1.5f, Vector2.right);
            s.Tick(1.8f, Vector2.right);
            s.Reset(1.9f);
            Assert.AreEqual(SwoopPhase.Hover, s.Phase);
            Assert.AreEqual(Vector2.zero, s.Tick(2.0f, Vector2.right));
        }
    }
}
