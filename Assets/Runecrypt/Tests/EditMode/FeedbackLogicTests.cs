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
        [Test] public void Offset_ZeroAtZero_FourAtOne() { var t = new ShakeTrauma(); Assert.AreEqual(0f, t.OffsetPixels); t.Add(1f); Assert.AreEqual(4f, t.OffsetPixels, 1e-5); }
        [Test] public void Offset_IsLinear() { var t = new ShakeTrauma(); t.Add(0.5f); Assert.AreEqual(2f, t.OffsetPixels, 1e-5); }
        // Review M1.5 Important #1: guncangan dibulatkan per piksel, jadi kejadian terkecil (mati, 0,25) harus ≥ 1 px.
        [Test] public void Offset_SmallestEventIsVisible() { var t = new ShakeTrauma(); t.Add(0.25f); Assert.GreaterOrEqual(t.OffsetPixels, 1f); }

        // --- Knockback (review Important #2): dorongan menekan kejaran, jadi musuh benar-benar mundur ---
        [Test] public void Knock_Full_SuppressesChase() { var v = Knockback.Combine(new UnityEngine.Vector2(-2f, 0f), new UnityEngine.Vector2(4f, 0f), 4f); Assert.AreEqual(4f, v.x, 1e-5); }
        [Test] public void Knock_Half_HalvesChase() { var v = Knockback.Combine(new UnityEngine.Vector2(-2f, 0f), new UnityEngine.Vector2(2f, 0f), 4f); Assert.AreEqual(1f, v.x, 1e-5); }
        [Test] public void Knock_None_KeepsChase() { var v = Knockback.Combine(new UnityEngine.Vector2(-2f, 0f), UnityEngine.Vector2.zero, 4f); Assert.AreEqual(-2f, v.x, 1e-5); }

        // --- Angka damage ---
        [TestCase(3.75f, 4)] [TestCase(3f, 3)] [TestCase(1.25f, 1)] [TestCase(0.4f, 1)] [TestCase(2.5f, 3)]
        public void Value_Rounds(float dmg, int expected) => Assert.AreEqual(expected, DamageNumberFormat.Value(dmg));
        [Test] public void Lit_IsBiggerThanDark() => Assert.Greater(DamageNumberFormat.Style(HitKind.Lit).scale, DamageNumberFormat.Style(HitKind.Dark).scale);

        // --- Batas suara ---
        [Test] public void Voice_UsesFreeSlotsFirst() { var v = new VoiceLimiter(3, 4); Assert.AreEqual(0, v.Acquire(1, 0f, 1f)); Assert.AreEqual(1, v.Acquire(2, 0f, 1f)); Assert.AreEqual(2, v.Acquire(3, 0f, 1f)); }
        [Test] public void Voice_ReusesFinishedSlot() { var v = new VoiceLimiter(2, 4); v.Acquire(1, 0f, 0.1f); v.Acquire(2, 0f, 1f); Assert.AreEqual(0, v.Acquire(3, 0.2f, 1f)); }
        [Test] public void Voice_GlobalFull_StealsOldest() { var v = new VoiceLimiter(2, 4); v.Acquire(1, 0f, 5f); v.Acquire(2, 0.1f, 5f); Assert.AreEqual(0, v.Acquire(3, 0.2f, 5f)); }
        // Review Important #4: klip yang sama dalam 30 ms menumpuk koheren (+12 dB untuk 4 salinan); lewati saja.
        [Test] public void Voice_SameClipWithin30ms_IsSkipped() { var v = new VoiceLimiter(12, 4); v.Acquire(5, 1f, 1f); Assert.AreEqual(-1, v.Acquire(5, 1.02f, 1f)); Assert.AreNotEqual(-1, v.Acquire(5, 1.04f, 1f)); }
        [Test] public void Voice_PerClipCap_StealsOldestOfSameClip() { var v = new VoiceLimiter(12, 2); v.Acquire(9, 0f, 5f); v.Acquire(7, 0.05f, 5f); v.Acquire(9, 0.1f, 5f); Assert.AreEqual(0, v.Acquire(9, 0.2f, 5f)); }
    }
}
