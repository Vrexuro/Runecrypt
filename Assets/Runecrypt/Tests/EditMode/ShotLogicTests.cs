using System.Collections.Generic;
using NUnit.Framework;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Tests
{
    // Sumber cahaya palsu untuk test (dipakai juga di test PlayMode M2).
    class TestLight : ILightSource
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public bool IsOn { get; set; } = true;
    }

    // Logika tembakan murni M2: sebar 10° (Bara Kembar), homing hanya ke musuh terang (GDD §5.5 aturan 2).
    public class ShotLogicTests
    {
        static float Angle(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        [Test]
        public void Spread_OneShot_IsAim()
        {
            var d = SpreadPattern.Directions(new Vector2(0f, 2f), 1, 10f);
            Assert.AreEqual(1, d.Length);
            Assert.AreEqual(90f, Angle(d[0]), 0.01f);
            Assert.AreEqual(1f, d[0].magnitude, 1e-4f);
        }

        [Test]
        public void Spread_TwoShots_Are10DegreesApart()
        {
            var d = SpreadPattern.Directions(Vector2.right, 2, 10f);
            Assert.AreEqual(10f, Vector2.Angle(d[0], d[1]), 0.01f);
            Assert.AreEqual(-Angle(d[0]), Angle(d[1]), 0.01f, "simetris terhadap arah bidik");
        }

        [Test]
        public void Nearest_IgnoresDarkPoints()
        {
            var pts = new List<Vector2> { new Vector2(1f, 0f), new Vector2(3f, 0f) };
            int i = TargetSelector.Nearest(Vector2.zero, pts, 6f, p => p.x > 2f);
            Assert.AreEqual(1, i);
        }

        [Test]
        public void Nearest_OutOfRange_ReturnsMinusOne()
        {
            var pts = new List<Vector2> { new Vector2(10f, 0f) };
            Assert.AreEqual(-1, TargetSelector.Nearest(Vector2.zero, pts, 6f, _ => true));
        }

        [Test]
        public void Steer_ClampsTurn()
        {
            var v = Homing.Steer(new Vector2(10f, 0f), new Vector2(0f, 1f), 30f);
            Assert.AreEqual(30f, Angle(v), 0.01f);
            Assert.AreEqual(10f, v.magnitude, 1e-3f);
        }

        [Test]
        public void Steer_SmallAngle_Snaps()
        {
            var to = new Vector2(Mathf.Cos(5f * Mathf.Deg2Rad), Mathf.Sin(5f * Mathf.Deg2Rad));
            var v = Homing.Steer(new Vector2(10f, 0f), to, 30f);
            Assert.AreEqual(5f, Angle(v), 0.01f);
        }

        [Test]
        public void IsLit_IgnoresGivenSource()
        {
            var f = new LightField((a, b) => false);
            var own = new TestLight { Radius = 5f };
            f.Register(own);
            Assert.IsTrue(f.IsLit(new Vector2(1f, 0f)));
            Assert.IsFalse(f.IsLit(new Vector2(1f, 0f), own), "cahaya bola api sendiri tidak dihitung");
        }

        [Test]
        public void PickUnowned_SkipsOwned()
        {
            var a = ScriptableObject.CreateInstance<RelicDefinition>();
            var b = ScriptableObject.CreateInstance<RelicDefinition>();
            var pool = new List<RelicDefinition> { a, b };
            var owned = new List<RelicDefinition> { a };
            for (int seed = 0; seed < 20; seed++)
                Assert.AreSame(b, RelicPicker.PickUnowned(pool, owned, new System.Random(seed)));
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }

        [Test]
        public void PickUnowned_AllOwned_ReturnsNull()
        {
            var a = ScriptableObject.CreateInstance<RelicDefinition>();
            var pool = new List<RelicDefinition> { a };
            Assert.IsNull(RelicPicker.PickUnowned(pool, pool, new System.Random(1)));
            Assert.IsNull(RelicPicker.PickUnowned(new List<RelicDefinition>(), pool, new System.Random(1)));
            Object.DestroyImmediate(a);
        }
    }
}
