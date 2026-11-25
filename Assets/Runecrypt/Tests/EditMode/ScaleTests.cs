using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

        // Gelombang penuh (8): gelombang berikutnya saat sisa 4.
        [Test]
        public void Waves_NextInRoomWhenFourLeft()
        {
            var s = After(14);
            Assert.IsFalse(s.ShouldStartNextWave(5, 0));
            Assert.IsTrue(s.ShouldStartNextWave(4, 0));
            Assert.IsFalse(s.ShouldStartNextWave(4, 2), "gelombang sebelumnya belum habis keluar");
        }

        // Review akhir: gelombang kecil (3) tidak langsung disusul gelombang berikutnya; butuh separuhnya terbunuh.
        [Test]
        public void Waves_SmallWaveNeedsKills()
        {
            var s = After(1);
            Assert.IsFalse(s.ShouldStartNextWave(3, 0));
            Assert.IsFalse(s.ShouldStartNextWave(2, 0));
            Assert.IsTrue(s.ShouldStartNextWave(1, 0));
        }

        // Review akhir M3a: aturan separuh memakai jumlah musuh yang benar-benar keluar, bukan budget.
        // Ruangan 2 gelombang 1: budget 4 bisa menjadi 2 Biarawan (biaya 2), jadi ambangnya 1, bukan 2.
        [Test]
        public void Waves_HalfRuleUsesSpawnedCount()
        {
            var s = After(3);
            s.AdvanceWave(2);
            Assert.IsFalse(s.ShouldStartNextWave(2, 0), "belum ada yang terbunuh dari 2 musuh");
            Assert.IsTrue(s.ShouldStartNextWave(1, 0));
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

        // ---------- Task 6: efek heboh yang tetap terbaca ----------
        static readonly string[] PlayerEffects = { "MuzzleFlash", "HitBurst", "EmberBurst", "BlazeRing", "BlazeSparks", "Fireball" };

        static GameObject Fx(string name)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");
            Assert.IsNotNull(go, name + " ada");
            return go;
        }

        [Test]
        public void EffectPrefabs_FlaresShortAndShadowless()
        {
            foreach (var name in new[] { "MuzzleFlash", "HitBurst", "BlazeRing" })
                Assert.IsNotEmpty(Fx(name).GetComponentsInChildren<FlareFade>(true), name + " punya kilatan");
            foreach (var name in PlayerEffects)
            foreach (var flare in Fx(name).GetComponentsInChildren<FlareFade>(true))
            {
                Assert.LessOrEqual(Serialized(flare, "duration"), 0.2f, name);
                Assert.IsFalse(flare.GetComponent<Light2D>().shadowsEnabled, name + " tanpa bayangan");
                Assert.IsNull(flare.GetComponent<LightSourceBehaviour>(), name + " tidak ikut aturan cahaya");
            }
        }

        [Test]
        public void EffectPrefabs_VisualsShort()
        {
            foreach (var name in PlayerEffects)
            {
                foreach (var ps in Fx(name).GetComponentsInChildren<ParticleSystem>(true))
                    Assert.LessOrEqual(ps.main.startLifetime.constantMax, 0.4f, name + "/" + ps.name);
                foreach (var fade in Fx(name).GetComponentsInChildren<ExpandFade>(true))
                    Assert.LessOrEqual(Serialized(fade, "duration"), 0.4f, name + "/" + fade.name);
            }
        }

        // Spec M2.6 §5.4: mata musuh selalu di atas semua efek pemain.
        [Test]
        public void EnemyEyes_DrawAboveAllEffects()
        {
            int top = int.MinValue;
            foreach (var name in PlayerEffects)
            foreach (var r in Fx(name).GetComponentsInChildren<Renderer>(true))
                top = Mathf.Max(top, r.sortingOrder);
            var eyes = Fx("TulangPenjaga").transform.Find("Eyes").GetComponent<SpriteRenderer>();
            Assert.Greater(eyes.sortingOrder, top);
        }
    }
}
