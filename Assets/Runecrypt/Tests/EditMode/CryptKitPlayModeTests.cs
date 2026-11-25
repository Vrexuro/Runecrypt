using System.Collections;
using System.Linq;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // M2.5 Isi Makam. Play mode di test runner memakai scene kosong, jadi setiap test membangun objeknya sendiri
    // di x ≥ 100.
    public class CryptKitPlayModeTests
    {
        const float X = 100f;

        static IEnumerator WaitUntilOrTimeout(System.Func<bool> done, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!done() && Time.realtimeSinceStartup < end) yield return null;
        }

        internal static PlayerController MakePlayer(Vector2 pos)
        {
            var go = new GameObject("TestPlayer");
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            return go.AddComponent<PlayerController>();
        }

        // Templat musuh diam jauh di luar arena uji; yang dibunuh hanya salinannya.
        static (ArenaDirector director, Enemy template) MakeDirector(int maxAlive = 12)
        {
            MakePlayer(new Vector2(X, 0f));
            var template = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 100f, 0f), hp: 5f);
            var spawn = new GameObject("Spawn").transform;
            spawn.position = new Vector2(X + 6f, 0f);
            var dirGo = new GameObject("Director");
            dirGo.SetActive(false);
            var director = dirGo.AddComponent<ArenaDirector>();
            var so = new SerializedObject(director);
            so.FindProperty("enemyPrefab").objectReferenceValue = template;
            var points = so.FindProperty("spawnPoints");
            points.arraySize = 1;
            points.GetArrayElementAtIndex(0).objectReferenceValue = spawn;
            so.FindProperty("waveDelay").floatValue = 0.5f;
            so.FindProperty("maxAlive").intValue = maxAlive;
            so.ApplyModifiedPropertiesWithoutUndo();
            return (director, template);
        }

        // M2.6 Task 3: satu ruangan = 3 gelombang; Started hanya di awal ruangan.
        [UnityTest]
        public IEnumerator ArenaDirector_RaisesRoomStartedEachRoom()
        {
            yield return new EnterPlayMode();
            var (director, template) = MakeDirector();
            int count = 0, countAtWave3 = -1;
            RoomEvents.Started += () => count++;
            director.gameObject.SetActive(true);
            try
            {
                float end = Time.realtimeSinceStartup + 20f;
                while (count < 2 && Time.realtimeSinceStartup < end)
                {
                    if (director.Wave == 3 && countAtWave3 < 0) countAtWave3 = count;
                    foreach (var e in Enemy.Active.Where(e => e != template && e.enabled).ToList())
                        e.TakeHit(new HitInfo(999f, Element.None, false, e.transform.position));
                    yield return null;
                }
                Assert.AreEqual(1, countAtWave3, "gelombang 1–3 satu ruangan");
                Assert.AreEqual(2, count, "ruangan kedua dimulai");
                Assert.AreEqual(4, director.Wave, "ruangan kedua dimulai di gelombang 4");
            }
            finally { RoomEvents.ClearForTests(); }
            yield return new ExitPlayMode();
        }

        // M2.6 Task 3 + review akhir: gelombang 2 menunggu musuh terbunuh, lalu tertahan batas musuh hidup,
        // dan sisanya keluar setelah ada yang mati.
        [UnityTest]
        public IEnumerator ArenaDirector_RespectsAliveCap()
        {
            yield return new EnterPlayMode();
            var (director, template) = MakeDirector(maxAlive: 3);
            director.gameObject.SetActive(true);
            System.Func<Enemy[]> live = () => FindAll().Where(e => e != template && e.enabled).ToArray();
            yield return WaitUntilOrTimeout(() => live().Length == 3, 5f);
            float settle = Time.realtimeSinceStartup + 0.5f;
            while (Time.realtimeSinceStartup < settle) yield return null;
            int waveBeforeKills = director.Wave;
            foreach (var e in live().Take(2)) e.TakeHit(new HitInfo(999f, Element.None, false, e.transform.position));
            yield return WaitUntilOrTimeout(() => director.Wave == 2 && director.Pending > 0, 5f);
            int pending = director.Pending;
            yield return WaitUntilOrTimeout(() => live().Length == 3, 5f);
            var one = live()[0];
            one.TakeHit(new HitInfo(999f, Element.None, false, one.transform.position));
            yield return WaitUntilOrTimeout(() => director.Pending < pending, 5f);
            int pendingAfter = director.Pending;
            yield return new ExitPlayMode();
            Assert.AreEqual(1, waveBeforeKills, "gelombang 1 (3 musuh) tidak langsung disusul gelombang 2");
            Assert.Greater(pending, 0, "gelombang 2 (4) tertahan batas 3");
            Assert.Less(pendingAfter, pending, "sisa gelombang 2 keluar setelah ada yang mati");
        }

        static Enemy[] FindAll() => Object.FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        static (CameraFollow follow, Camera cam) MakeRig(Transform target)
        {
            var rig = new GameObject("CameraRig");
            rig.SetActive(false);
            var follow = rig.AddComponent<CameraFollow>();
            var so = new SerializedObject(follow);
            so.FindProperty("target").objectReferenceValue = target;
            so.FindProperty("room").rectValue = new Rect(X - 15f, -9f, 30f, 17f);
            so.FindProperty("smoothTime").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
            var camGo = new GameObject("Cam");
            camGo.transform.SetParent(rig.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.625f;
            cam.aspect = 16f / 9f;
            camGo.AddComponent<CameraShake>();
            rig.SetActive(true);
            return (follow, cam);
        }

        [UnityTest]
        public IEnumerator CameraFollow_TracksPlayerWithinRoom()
        {
            yield return new EnterPlayMode();
            var target = new GameObject("Target").transform;
            target.position = new Vector2(X + 2f, 1f);
            var (follow, _) = MakeRig(target);
            yield return null; yield return null;
            Vector2 mid = follow.transform.position;
            target.position = new Vector2(X + 40f, 40f);
            yield return null; yield return null;
            Vector2 corner = follow.transform.position;
            yield return new ExitPlayMode();
            Assert.AreEqual(X + 2f, mid.x, 1e-3f);
            Assert.AreEqual(1f, mid.y, 1e-3f);
            Assert.AreEqual(X + 5f, corner.x, 1e-3f, "tepi kanan tampilan = tepi ruangan");
            Assert.AreEqual(2.375f, corner.y, 1e-3f);
        }

        // Review akhir: layar bukan 16:9 (mis. 1366x768 memperlihatkan area lebih luas); batas dihitung dari kamera.
        [UnityTest]
        public IEnumerator CameraFollow_UsesActualViewAtCorners()
        {
            yield return new EnterPlayMode();
            var target = new GameObject("Target").transform;
            target.position = new Vector2(X + 40f, 40f);
            var (follow, cam) = MakeRig(target);
            cam.orthographicSize = 6f;
            cam.aspect = 2f;
            yield return null; yield return null;
            Vector2 corner = follow.transform.position;
            yield return new ExitPlayMode();
            Assert.AreEqual(X + 3f, corner.x, 1e-3f, "tepi kanan tampilan (setengah lebar 12) = tepi ruangan");
            Assert.AreEqual(2f, corner.y, 1e-3f, "tepi atas tampilan (setengah tinggi 6) = tepi ruangan");
        }

        // Review Focus 2: guncangan tidak menggeser kamera secara permanen dari posisi ikut.
        [UnityTest]
        public IEnumerator Shake_ReturnsToFollowPosition()
        {
            yield return new EnterPlayMode();
            var target = new GameObject("Target").transform;
            target.position = new Vector2(X, 0f);
            var (follow, cam) = MakeRig(target);
            var shake = cam.GetComponent<CameraShake>();
            yield return null;
            shake.Add(1f);
            for (int i = 0; i < 5; i++) { target.position += Vector3.right * 0.3f; yield return null; }
            yield return WaitUntilOrTimeout(() => cam.transform.localPosition == new Vector3(0f, 0f, -10f), 5f);
            Vector3 local = cam.transform.localPosition;
            Vector2 rigPos = follow.transform.position, expected = target.position;
            yield return new ExitPlayMode();
            Assert.AreEqual(new Vector3(0f, 0f, -10f), local, "kamera anak kembali ke posisi dasar");
            Assert.AreEqual(expected.x, rigPos.x, 1e-3f, "rig tetap mengikuti pemain");
        }

        // ---------- Task 4: Anglo ----------
        const string Prefabs = "Assets/Runecrypt/Prefabs/";

        static T Spawn<T>(string prefab, Vector2 pos) where T : Component =>
            Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab"), pos, Quaternion.identity)
                .GetComponent<T>();

        static HitInfo Hit(Element e, Vector2 at, float damage = 3f) => new HitInfo(damage, e, e == Element.Fire, at);

        [UnityTest]
        public IEnumerator Fireball_LightsBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X + 3f, 0f));
            yield return ProjectilePlayModeTests.Steps(2);
            ProjectilePlayModeTests.MakeFireball(new Vector2(X, 0f)).Launch(Vector2.right);
            yield return ProjectilePlayModeTests.Steps(30);
            bool lit = brazier.IsLit;
            yield return new ExitPlayMode();
            Assert.IsTrue(lit);
        }

        // Anglo yang menyala adalah sumber cahaya aturan dengan radius 2,5 (spec §5.2).
        [UnityTest]
        public IEnumerator LitBrazier_RevealsNearbyEnemy()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            Vector2 near = new Vector2(X + 2f, 0f), far = new Vector2(X + 3f, 0f);
            bool before = LightFieldHost.Field.IsLit(near);
            brazier.TakeHit(Hit(Element.Fire, brazier.transform.position));
            bool nearAfter = LightFieldHost.Field.IsLit(near), farAfter = LightFieldHost.Field.IsLit(far);
            yield return new ExitPlayMode();
            Assert.IsFalse(before, "padam: tidak menerangi");
            Assert.IsTrue(nearAfter, "2 tile: terang");
            Assert.IsFalse(farAfter, "3 tile: di luar radius 2,5");
        }

        [UnityTest]
        public IEnumerator IceHit_DoesNotLightBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            brazier.TakeHit(Hit(Element.Ice, brazier.transform.position));
            bool lit = brazier.IsLit;
            yield return new ExitPlayMode();
            Assert.IsFalse(lit);
        }

        [UnityTest]
        public IEnumerator RoomStarted_ExtinguishesBrazier()
        {
            yield return new EnterPlayMode();
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 0f));
            brazier.TakeHit(Hit(Element.Fire, brazier.transform.position));
            RoomEvents.RaiseStarted();
            bool lit = brazier.IsLit, lightOn = LightFieldHost.Field.IsLit(new Vector2(X + 1f, 0f));
            yield return new ExitPlayMode();
            Assert.IsFalse(lit);
            Assert.IsFalse(lightOn, "sumber cahaya ikut padam");
        }

        // ---------- Task 5: Guci Abu dan Bara Pemulih ----------
        static PlayerController PlayerAt(Vector2 pos)
        {
            var p = MakePlayer(pos);
            p.gameObject.layer = LayerMask.NameToLayer("Player");
            return p;
        }

        [UnityTest]
        public IEnumerator Fireball_BreaksUrnAndIsSpent()
        {
            yield return new EnterPlayMode();
            var urn = Spawn<AshUrn>("AshUrn", new Vector2(X + 3f, 0f));
            yield return ProjectilePlayModeTests.Steps(2);
            var fb = ProjectilePlayModeTests.MakeFireball(new Vector2(X, 0f));
            fb.Launch(Vector2.right);
            yield return ProjectilePlayModeTests.Steps(30);
            bool broken = urn.IsBroken, spent = fb == null;
            yield return new ExitPlayMode();
            Assert.IsTrue(broken, "Bola Api dasar (damage 3) memecahkan guci HP 3");
            Assert.IsTrue(spent, "tembakan habis");
        }

        [UnityTest]
        public IEnumerator Urn_DropsEmberWhenRollUnderChance()
        {
            yield return new EnterPlayMode();
            var lucky = Spawn<AshUrn>("AshUrn", new Vector2(X, 0f));
            lucky.random = () => 0.1f;
            lucky.TakeHit(Hit(Element.Fire, lucky.transform.position));
            var unlucky = Spawn<AshUrn>("AshUrn", new Vector2(X + 10f, 0f));
            unlucky.random = () => 0.5f;
            unlucky.TakeHit(Hit(Element.Fire, unlucky.transform.position));
            var embers = Object.FindObjectsByType<HealingEmber>(FindObjectsSortMode.None);
            int nearLucky = embers.Count(e => Vector2.Distance(e.transform.position, lucky.transform.position) < 1f);
            int nearUnlucky = embers.Count(e => Vector2.Distance(e.transform.position, unlucky.transform.position) < 1f);
            yield return new ExitPlayMode();
            Assert.AreEqual(1, nearLucky, "undian 0,1 < 0,15: jatuh");
            Assert.AreEqual(0, nearUnlucky, "undian 0,5: tidak jatuh");
        }

        [UnityTest]
        public IEnumerator HealingEmber_HealsOneCappedAtMax()
        {
            yield return new EnterPlayMode();
            var hurt = PlayerAt(new Vector2(X + 20f, 0f));
            hurt.Health.TakeDamage(2f);                                   // 4/6
            var full = PlayerAt(new Vector2(X + 30f, 0f));                // 6/6
            var e1 = Spawn<HealingEmber>("HealingEmber", hurt.transform.position);
            var e2 = Spawn<HealingEmber>("HealingEmber", full.transform.position);
            yield return ProjectilePlayModeTests.Steps(3);
            float hurtHp = hurt.Health.Current, fullHp = full.Health.Current;
            bool gone1 = e1 == null, gone2 = e2 == null;
            yield return new ExitPlayMode();
            Assert.AreEqual(5f, hurtHp, 1e-4f);
            Assert.AreEqual(6f, fullHp, 1e-4f, "tidak melewati HP maksimal");
            Assert.IsTrue(gone1 && gone2, "bara habis saat diambil");
        }

        [UnityTest]
        public IEnumerator HealingEmber_ExpiresAfterTenSeconds()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<HealingEmber>(Prefabs + "HealingEmber.prefab");
            float lifetime = new SerializedObject(prefab).FindProperty("lifetime").floatValue;
            yield return new EnterPlayMode();
            var ember = Spawn<HealingEmber>("HealingEmber", new Vector2(X, 0f));
            var so = new SerializedObject(ember);
            so.FindProperty("lifetime").floatValue = 0.2f; // diperpendek supaya test cepat
            so.ApplyModifiedPropertiesWithoutUndo();
            yield return WaitUntilOrTimeout(() => ember == null, 3f);
            bool gone = ember == null;
            yield return new ExitPlayMode();
            Assert.AreEqual(10f, lifetime, 1e-4f, "spec §5.3: 10 detik");
            Assert.IsTrue(gone);
        }

        [UnityTest]
        public IEnumerator RoomStarted_RestoresUrn()
        {
            yield return new EnterPlayMode();
            var urn = Spawn<AshUrn>("AshUrn", new Vector2(X, 0f));
            urn.random = () => 1f;
            urn.TakeHit(Hit(Element.Fire, urn.transform.position));
            RoomEvents.RaiseStarted();
            yield return ProjectilePlayModeTests.Steps(2);
            bool broken = urn.IsBroken;
            yield return new ExitPlayMode();
            Assert.IsFalse(broken);
        }

        // Review Focus 1: guci yang muncul lagi tidak boleh menjebak pemain di dalam collider.
        [UnityTest]
        public IEnumerator AshUrn_RespawnWaitsUntilPlayerClear()
        {
            yield return new EnterPlayMode();
            var urn = Spawn<AshUrn>("AshUrn", new Vector2(X, 0f));
            urn.random = () => 1f;
            urn.TakeHit(Hit(Element.Fire, urn.transform.position));
            var player = PlayerAt(urn.transform.position);
            yield return ProjectilePlayModeTests.Steps(2);
            RoomEvents.RaiseStarted();
            yield return ProjectilePlayModeTests.Steps(3);
            bool whileStanding = urn.IsBroken;
            player.transform.position = new Vector2(X + 5f, 0f);
            Physics2D.SyncTransforms();
            yield return ProjectilePlayModeTests.Steps(3);
            bool afterLeaving = urn.IsBroken;
            yield return new ExitPlayMode();
            Assert.IsTrue(whileStanding, "menunggu selama pemain berdiri di sana");
            Assert.IsFalse(afterLeaving, "utuh lagi setelah pemain pergi");
        }

        // Review akhir Important 2: pemain yang hanya sebagian menempel di guci juga harus ditunggu.
        [UnityTest]
        public IEnumerator AshUrn_RespawnWaitsForPartlyOverlappingPlayer()
        {
            yield return new EnterPlayMode();
            var urn = Spawn<AshUrn>("AshUrn", new Vector2(X, 0f));
            urn.random = () => 1f;
            urn.TakeHit(Hit(Element.Fire, urn.transform.position));
            PlayerAt(new Vector2(X + 0.5f, 0f)); // radius 0,35: tepi pemain masuk ke kotak guci (setengah lebar 0,3)
            yield return ProjectilePlayModeTests.Steps(2);
            RoomEvents.RaiseStarted();
            yield return ProjectilePlayModeTests.Steps(3);
            bool waiting = urn.IsBroken;
            yield return new ExitPlayMode();
            Assert.IsTrue(waiting, "guci belum utuh selama pemain masih menyentuh tempatnya");
        }

        // Review Focus 2: tembakan pierce tetap tembus guci dan mengenai musuh di belakangnya.
        [UnityTest]
        public IEnumerator PiercingShot_PassesUrnAndHitsEnemyBehind()
        {
            yield return new EnterPlayMode();
            Spawn<AshUrn>("AshUrn", new Vector2(X + 2f, 0f));
            var enemy = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 4f, 0f), 100f);
            yield return ProjectilePlayModeTests.Steps(2);
            var shot = new ShotStats { Damage = 3f, Speed = 12f, Range = 7f, Size = 1f, LightScale = 1f, Count = 1, Pierce = 1 };
            ProjectilePlayModeTests.MakeFireball(new Vector2(X, 0f)).Launch(Vector2.right, shot);
            yield return ProjectilePlayModeTests.Steps(40);
            float hp = enemy.CurrentHp;
            yield return new ExitPlayMode();
            Assert.Less(hp, 100f, "musuh di belakang guci tetap kena");
        }

        // Review Focus 3: Kobaran mengenai semua benda dalam radiusnya.
        [UnityTest]
        public IEnumerator BlazeRing_BreaksEveryUrnInRadius()
        {
            yield return new EnterPlayMode();
            var a = Spawn<AshUrn>("AshUrn", new Vector2(X + 1f, 0f));
            var b = Spawn<AshUrn>("AshUrn", new Vector2(X - 1f, 0f));
            var brazier = Spawn<Brazier>("Brazier", new Vector2(X, 1.5f));
            a.random = b.random = () => 1f;
            yield return ProjectilePlayModeTests.Steps(2);
            var ring = new GameObject("Blaze").AddComponent<BlazeRing>();
            ring.transform.position = new Vector2(X, 0f);
            ring.Init(6f, 2.5f, 2f, 1f);
            bool both = a.IsBroken && b.IsBroken, lit = brazier.IsLit;
            yield return new ExitPlayMode();
            Assert.IsTrue(both, "kedua guci pecah");
            Assert.IsTrue(lit, "Anglo dalam radius menyala");
        }

        // ---------- Task 6: Nisan dan Sarkofagus ----------
        [UnityTest]
        public IEnumerator Tombstone_StopsFireball()
        {
            yield return new EnterPlayMode();
            Spawn<Transform>("Tombstone", new Vector2(X + 3f, 0f));
            yield return ProjectilePlayModeTests.Steps(2);
            var fb = ProjectilePlayModeTests.MakeFireball(new Vector2(X, 0f));
            fb.Launch(Vector2.right);
            yield return ProjectilePlayModeTests.Steps(30);
            bool stopped = fb == null;
            yield return new ExitPlayMode();
            Assert.IsTrue(stopped, "Bola Api berhenti di nisan, tidak terbang sampai jangkauan habis");
        }

        // Nisan di layer Walls juga menghalangi aturan cahaya (raycast LightFieldHost).
        [UnityTest]
        public IEnumerator Tombstone_BlocksLightRule()
        {
            yield return new EnterPlayMode();
            var hostGo = new GameObject("LightFieldHost");
            hostGo.SetActive(false);
            var host = hostGo.AddComponent<LightFieldHost>();
            var hso = new SerializedObject(host);
            hso.FindProperty("wallMask").intValue = LayerMask.GetMask("Walls");
            hso.ApplyModifiedPropertiesWithoutUndo();
            hostGo.SetActive(true);
            var lamp = new GameObject("Lamp");
            lamp.transform.position = new Vector2(X, 0f);
            lamp.AddComponent<LightSourceBehaviour>().Radius = 3f;
            Spawn<Transform>("Tombstone", new Vector2(X + 1.5f, 0f));
            yield return ProjectilePlayModeTests.Steps(2);
            Physics2D.SyncTransforms();
            bool behind = LightFieldHost.Field.IsLit(new Vector2(X + 2.5f, 0f));
            bool beside = LightFieldHost.Field.IsLit(new Vector2(X, 2.5f));
            Object.Destroy(lamp);
            yield return new ExitPlayMode();
            Assert.IsFalse(behind, "titik di balik nisan gelap");
            Assert.IsTrue(beside, "titik pada jarak sama tanpa penghalang terang");
        }

        // M2.6 Review Focus 4: kilatan efek terlihat terang tetapi tidak membuat musuh Tersingkap.
        [UnityTest]
        public IEnumerator HitFlare_DoesNotRevealEnemy()
        {
            yield return new EnterPlayMode();
            Spawn<Transform>("HitBurst", new Vector2(X, 0f));
            yield return null;
            bool lit = LightFieldHost.Field.IsLit(new Vector2(X + 0.5f, 0f));
            yield return new ExitPlayMode();
            Assert.IsFalse(lit);
        }

        // Review Focus 4: musuh yang menyentuh Bara Pemulih tidak menghabiskannya.
        [UnityTest]
        public IEnumerator HealingEmber_IgnoresEnemies()
        {
            yield return new EnterPlayMode();
            var ember = Spawn<HealingEmber>("HealingEmber", new Vector2(X, 0f));
            ProjectilePlayModeTests.MakeEnemy(new Vector2(X, 0f), 10f);
            yield return ProjectilePlayModeTests.Steps(5);
            bool present = ember != null;
            yield return new ExitPlayMode();
            Assert.IsTrue(present);
        }
    }
}
