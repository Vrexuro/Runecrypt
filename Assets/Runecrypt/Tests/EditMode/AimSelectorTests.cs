using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    public class AimSelectorTests
    {
        [Test]
        public void Select_StickWins()
        {
            var r = AimSelector.Select(new Vector2(1, 0), new Vector2(0, 1), new Vector2(5, 5), Vector2.zero, Vector2.up);
            Assert.AreEqual(Vector2.right, r.Direction);
            Assert.IsTrue(r.AutoFire);
        }

        [Test]
        public void Select_KeysWhenStickInDeadzone()
        {
            var r = AimSelector.Select(new Vector2(0.1f, 0), new Vector2(0, 1), new Vector2(5, 5), Vector2.zero, Vector2.right);
            Assert.AreEqual(Vector2.up, r.Direction);
            Assert.IsTrue(r.AutoFire);
        }

        [Test]
        public void Select_DiagonalKeys_AreNormalized()
        {
            var r = AimSelector.Select(Vector2.zero, new Vector2(1, 1), Vector2.zero, Vector2.zero, Vector2.right);
            Assert.AreEqual(1f, r.Direction.magnitude, 1e-4);
        }

        [Test]
        public void Select_MouseFallback()
        {
            var r = AimSelector.Select(Vector2.zero, Vector2.zero, new Vector2(0, 4), Vector2.zero, Vector2.right);
            Assert.AreEqual(Vector2.up, r.Direction);
            Assert.IsFalse(r.AutoFire);
        }

        [Test]
        public void Select_MouseOnPlayer_KeepsLastAim()
        {
            var r = AimSelector.Select(Vector2.zero, Vector2.zero, new Vector2(2, 2), new Vector2(2, 2), Vector2.left);
            Assert.AreEqual(Vector2.left, r.Direction);
        }
    }
}
