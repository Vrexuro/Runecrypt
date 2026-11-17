using NUnit.Framework;
using Runecrypt.Core;

namespace Runecrypt.Tests
{
    // Logika murni umpan balik M1.5 (spec Docs/specs/2026-11-17-juice-design.md §5–6).
    public class FeedbackLogicTests
    {
        // --- Hit-stop ---
        [Test] public void Overlap_TakesLongestNotSum() { var c = new HitStopClock(); c.Request(0f, 0.03f); c.Request(0.01f, 0.03f); Assert.IsTrue(c.IsStopped(0.039f)); Assert.IsFalse(c.IsStopped(0.041f)); }
        [Test] public void EndsOnTime() { var c = new HitStopClock(); c.Request(1f, 0.08f); Assert.IsTrue(c.IsStopped(1.079f)); Assert.IsFalse(c.IsStopped(1.081f)); }
        [Test] public void NoRequest_RunsAtOne() => Assert.AreEqual(1f, new HitStopClock().TimeScaleAt(5f, false));
        [Test] public void Paused_AlwaysZero() { var c = new HitStopClock(); c.Request(0f, 0.05f); Assert.AreEqual(0f, c.TimeScaleAt(0.02f, true)); Assert.AreEqual(0f, c.TimeScaleAt(1f, true)); Assert.AreEqual(1f, c.TimeScaleAt(1f, false)); }

        // --- Guncangan ---
        [Test] public void Trauma_ClampedToOne() { var t = new ShakeTrauma(); t.Add(0.45f); t.Add(0.45f); t.Add(0.45f); Assert.AreEqual(1f, t.Value, 1e-5); }
        [Test] public void Trauma_DecaysToZeroNotBelow() { var t = new ShakeTrauma(); t.Add(0.3f); t.Tick(0.1f); Assert.AreEqual(0.15f, t.Value, 1e-5); t.Tick(1f); Assert.AreEqual(0f, t.Value); }
        [Test] public void Offset_ZeroAtZero_ThreeAtOne() { var t = new ShakeTrauma(); Assert.AreEqual(0f, t.OffsetPixels); t.Add(1f); Assert.AreEqual(3f, t.OffsetPixels, 1e-5); }
        [Test] public void Offset_IsQuadratic() { var t = new ShakeTrauma(); t.Add(0.5f); Assert.AreEqual(0.75f, t.OffsetPixels, 1e-5); }
    }
}
