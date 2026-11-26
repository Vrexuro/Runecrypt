using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    // Sprite mage menghadap kanan; flip = hadap kiri.
    public class FacingTests
    {
        [Test]
        public void AttackLeaningLeft_FacesLeft()
        {
            Assert.IsTrue(Facing.FlipForAttack(new Vector2(-0.6f, 0.8f), currentFlip: false));
        }

        [Test]
        public void AttackLeaningRight_FacesRight()
        {
            Assert.IsFalse(Facing.FlipForAttack(new Vector2(0.6f, -0.8f), currentFlip: true));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AttackStraightUpOrDown_KeepsCurrentFacing(bool current)
        {
            Assert.AreEqual(current, Facing.FlipForAttack(Vector2.up, current));
            Assert.AreEqual(current, Facing.FlipForAttack(new Vector2(0.001f, -1f), current));
        }
    }
}
