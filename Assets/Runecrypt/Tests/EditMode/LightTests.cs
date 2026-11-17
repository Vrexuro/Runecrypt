using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    public class LightTests
    {
        class FakeSource : ILightSource
        {
            public Vector2 Position { get; set; }
            public float Radius { get; set; }
            public bool IsOn { get; set; } = true;
        }

        static LightField Open() => new LightField((a, b) => false);

        [Test]
        public void IsLit_InsideRadius()
        {
            var f = Open();
            f.Register(new FakeSource { Radius = 3 });
            Assert.IsTrue(f.IsLit(new Vector2(2, 0)));
        }

        [Test]
        public void IsLit_OnEdge()
        {
            var f = Open();
            f.Register(new FakeSource { Radius = 3 });
            Assert.IsTrue(f.IsLit(new Vector2(3, 0)));
        }

        [Test]
        public void IsLit_OutsideRadius_False()
        {
            var f = Open();
            f.Register(new FakeSource { Radius = 3 });
            Assert.IsFalse(f.IsLit(new Vector2(3.01f, 0)));
        }

        [Test]
        public void IsLit_BehindWall_False()
        {
            var f = new LightField((a, b) => true);
            f.Register(new FakeSource { Radius = 3 });
            Assert.IsFalse(f.IsLit(new Vector2(1, 0)));
        }

        [Test]
        public void IsLit_SourceOff_False()
        {
            var f = Open();
            f.Register(new FakeSource { Radius = 3, IsOn = false });
            Assert.IsFalse(f.IsLit(new Vector2(1, 0)));
        }

        [Test]
        public void Unregister_RemovesSource()
        {
            var f = Open();
            var s = new FakeSource { Radius = 3 };
            f.Register(s);
            f.Unregister(s);
            Assert.IsFalse(f.IsLit(new Vector2(1, 0)));
        }

        [Test]
        public void Register_Twice_UnregisterOnce_Removes()
        {
            var f = Open();
            var s = new FakeSource { Radius = 3 };
            f.Register(s);
            f.Register(s);
            f.Unregister(s);
            Assert.IsFalse(f.IsLit(new Vector2(1, 0)));
        }

        [Test]
        public void IsLit_TwoSources()
        {
            var f = Open();
            f.Register(new FakeSource { Position = Vector2.zero, Radius = 1 });
            f.Register(new FakeSource { Position = new Vector2(10, 0), Radius = 2 });
            Assert.IsTrue(f.IsLit(new Vector2(9, 0)));
            Assert.IsFalse(f.IsLit(new Vector2(5, 0)));
        }

        [Test]
        public void IsLit_WallCheckUsesSourceToPoint()
        {
            Vector2 from = default, to = default;
            var f = new LightField((a, b) => { from = a; to = b; return false; });
            f.Register(new FakeSource { Position = new Vector2(1, 1), Radius = 3 });
            f.IsLit(new Vector2(2, 1));
            Assert.AreEqual(new Vector2(1, 1), from);
            Assert.AreEqual(new Vector2(2, 1), to);
        }
    }
}
