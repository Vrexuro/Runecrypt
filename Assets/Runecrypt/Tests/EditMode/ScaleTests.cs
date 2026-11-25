using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;

namespace Runecrypt.Tests
{
    // M2.6 Skala dan Efek (Docs/specs/2026-11-25-m2-6-skala-dan-efek-design.md).
    public class ScaleTests
    {
        // ---------- Task 1: jadwal gelombang ----------
        static WaveSchedule After(int waves)
        {
            var s = new WaveSchedule();
            for (int i = 0; i < waves; i++) s.AdvanceWave();
            return s;
        }

        [Test]
        public void Waves_RoomStartsEveryThirdWave()
        {
            var s = new WaveSchedule();
            for (int w = 1; w <= 7; w++)
            {
                s.AdvanceWave();
                Assert.AreEqual(w == 1 || w == 4 || w == 7, s.IsRoomStart, $"gelombang {w}");
            }
            Assert.AreEqual(3, s.Room);
            Assert.AreEqual(1, s.WaveInRoom);
        }

        [Test]
        public void Waves_FirstWaveWaitsForEmptyArena()
        {
            var s = new WaveSchedule();
            Assert.IsFalse(s.ShouldStartNextWave(1, 0));
            Assert.IsTrue(s.ShouldStartNextWave(0, 0));
        }

        [Test]
        public void Waves_NextInRoomWhenFourLeft()
        {
            var s = After(1);
            Assert.IsFalse(s.ShouldStartNextWave(5, 0));
            Assert.IsTrue(s.ShouldStartNextWave(4, 0));
            Assert.IsFalse(s.ShouldStartNextWave(4, 2), "gelombang sebelumnya belum habis keluar");
        }

        [Test]
        public void Waves_NextRoomOnlyWhenEmpty()
        {
            var s = After(3);
            Assert.IsFalse(s.ShouldStartNextWave(1, 0));
            Assert.IsTrue(s.ShouldStartNextWave(0, 0));
        }

        [Test]
        public void Waves_SizeCappedAndFirstRoomLight()
        {
            var s = new WaveSchedule();
            int firstRoom = 0;
            for (int w = 1; w <= 30; w++)
            {
                int size = s.NextWaveSize();
                Assert.That(size, Is.InRange(1, 8), $"gelombang {w}");
                s.AdvanceWave();
                if (s.Room == 1) firstRoom += size;
            }
            Assert.LessOrEqual(firstRoom, 12, "ruangan pertama ringan untuk pengunjung baru");
        }

        [Test]
        public void Waves_CapTwelve()
        {
            var s = new WaveSchedule();
            Assert.AreEqual(2, s.CanSpawnNow(10));
            Assert.AreEqual(0, s.CanSpawnNow(12));
            Assert.AreEqual(0, s.CanSpawnNow(13));
        }

        // Review Focus 3: ruangan belum selesai selama gelombangnya masih menunggu keluar.
        [Test]
        public void Waves_RoomNotDoneWhilePending()
        {
            var s = After(3);
            Assert.IsFalse(s.RoomDone(0, 3));
            Assert.IsFalse(s.RoomDone(1, 0));
            Assert.IsTrue(s.RoomDone(0, 0));
            Assert.IsFalse(After(2).RoomDone(0, 0), "gelombang 2 dari 3 belum akhir ruangan");
        }

        // ---------- Task 2: batas kamera ----------
        static readonly Rect Room = new Rect(-15f, -9f, 30f, 17f);
        static readonly Vector2 Half = new Vector2(10f, 5.625f); // 320x180 px pada 16 PPU

        [Test]
        public void CameraBounds_FollowsInMiddle() =>
            Assert.AreEqual(new Vector2(1f, -1f), CameraBounds.Clamp(new Vector2(1f, -1f), Room, Half));

        // Review Focus 1: di pojok, tepi tampilan berhenti di tepi ruangan.
        [Test]
        public void CameraBounds_ClampsAtCorners()
        {
            Assert.AreEqual(new Vector2(-5f, -3.375f), CameraBounds.Clamp(new Vector2(-20f, -20f), Room, Half));
            Assert.AreEqual(new Vector2(5f, 2.375f), CameraBounds.Clamp(new Vector2(20f, 20f), Room, Half));
        }

        [Test]
        public void CameraBounds_CentersWhenRoomSmallerThanView()
        {
            var small = new Rect(-10f, -6f, 20f, 11f); // arena lama 20x11: lebih pendek dari tampilan
            Vector2 c = CameraBounds.Clamp(new Vector2(3f, 3f), small, Half);
            Assert.AreEqual(0f, c.x, 1e-5f);
            Assert.AreEqual(-0.5f, c.y, 1e-5f);
        }

        [Test]
        public void SnapToPixel_RoundsToSixteenth()
        {
            Vector2 p = CameraBounds.SnapToPixel(new Vector2(1.03f, -0.95f), 16);
            Assert.AreEqual(1f, p.x, 1e-5f);
            Assert.AreEqual(-0.9375f, p.y, 1e-5f);
        }

        // ---------- Task 5: cahaya skill ----------
        const string Prefabs = "Assets/Runecrypt/Prefabs/";

        static T Prefab<T>(string name) where T : Component =>
            AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab").GetComponentInChildren<T>(true);

        static float Serialized(Object o, string field) => new SerializedObject(o).FindProperty(field).floatValue;

        [Test]
        public void FireballPrefab_LightRadius175() =>
            Assert.AreEqual(1.75f, Serialized(Prefab<LightSourceBehaviour>("Fireball"), "radius"), 1e-4f);

        [Test]
        public void BlazeRingPrefab_LightFourAndHalfForOneSecond()
        {
            var ring = Prefab<BlazeRing>("BlazeRing");
            Assert.AreEqual(4.5f, Serialized(ring, "lightRadius"), 1e-4f);
            Assert.AreEqual(1.0f, Serialized(ring, "lightTime"), 1e-4f);
        }

        // Penjaga: cahaya dasar pemain tidak ikut diperluas (spec M2.6 §5.3).
        [Test]
        public void PlayerPrefab_BaseLightStaysThree() =>
            Assert.AreEqual(3f, Serialized(Prefab<LightSourceBehaviour>("Player"), "radius"), 1e-4f);
    }
}
