using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // M2: modifier bola api dan relic. Berjalan di dalam scene M1 yang terbuka, jadi semua objek ditaruh di x ≥ 100
    // (jauh dari pemain dan lampu scene). Memakai langkah fisika sungguhan (WaitForFixedUpdate), karena pantulan,
    // jangkauan, dan homing ada di FixedUpdate, yang tidak dipanggil oleh Physics2D.Simulate.
    public class ProjectilePlayModeTests
    {
        const float X = 100f;

        internal static Enemy MakeEnemy(Vector2 pos, float hp)
        {
            var d = ScriptableObject.CreateInstance<EnemyDefinition>();
            d.maxHp = hp; d.moveSpeed = 0f;
            var go = new GameObject("TestEnemy");
            go.SetActive(false);
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var enemy = go.AddComponent<Enemy>();
            var so = new SerializedObject(enemy);
            so.FindProperty("definition").objectReferenceValue = d;
            so.FindProperty("lightRules").objectReferenceValue = ScriptableObject.CreateInstance<LightRules>();
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            return enemy;
        }

        static ShotStats Shot(int pierce = 0, int bounce = 0, float homing = 0f, float range = 7f) => new ShotStats
        {
            Damage = 3f, Speed = 12f, Range = range, Size = 1f, LightScale = 1f, BurnDuration = 2f, BurnDamagePerTick = 1f,
            Count = 1, Pierce = pierce, Bounce = bounce, HomingTurn = homing,
        };

        static Fireball MakeFireball(Vector2 pos, bool withLight = false)
        {
            var go = new GameObject("Fireball");
            go.SetActive(false);
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.2f;
            if (withLight)
            {
                var light = new GameObject("Light");
                light.transform.SetParent(go.transform, false);
                light.AddComponent<LightSourceBehaviour>().Radius = 3f;
            }
            var fb = go.AddComponent<Fireball>();
            go.SetActive(true);
            return fb;
        }

        static GameObject MakeWall(Vector2 pos)
        {
            var go = new GameObject("TestWall") { layer = LayerMask.NameToLayer("Walls") };
            go.transform.position = pos;
            go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 4f);
            return go;
        }

        // Menunggu n langkah fisika. WaitForFixedUpdate sendiri lanjut sekali per frame walau frame itu tanpa
        // langkah fisika, jadi yang dihitung adalah Time.fixedTime. Bisa lebih beberapa langkah saat frame melompat.
        static IEnumerator Steps(int n)
        {
            float target = Time.fixedTime + n * Time.fixedDeltaTime - 1e-4f;
            while (Time.fixedTime < target) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator Pierce1_HitsTwoEnemiesInLine()
        {
            yield return new EnterPlayMode();
            int deaths = 0;
            System.Action<Enemy> onDied = _ => deaths++;
            Enemy.AnyDied += onDied;
            MakeEnemy(new Vector2(X + 3f, 0f), 3f);
            MakeEnemy(new Vector2(X + 5f, 0f), 3f);
            yield return Steps(2);
            MakeFireball(new Vector2(X, 0f)).Launch(Vector2.right, Shot(pierce: 1));
            yield return Steps(40);
            Enemy.AnyDied -= onDied;
            yield return new ExitPlayMode();
            Assert.AreEqual(2, deaths, "pierce 1 = menembus satu musuh tambahan");
        }

        [UnityTest]
        public IEnumerator Pierce_DoesNotHitSameEnemyTwice()
        {
            yield return new EnterPlayMode();
            var a = MakeEnemy(new Vector2(X + 2f, 0f), 100f);
            var b = MakeEnemy(new Vector2(X + 4f, 0f), 100f);
            yield return Steps(2);
            MakeFireball(new Vector2(X, 0f)).Launch(Vector2.right, Shot(pierce: 3));
            yield return Steps(40);
            float lostA = 100f - a.CurrentHp, lostB = 100f - b.CurrentHp;
            yield return new ExitPlayMode();
            Assert.AreEqual(3f, lostA, 1e-4f, "musuh yang sama hanya dipukul sekali");
            Assert.AreEqual(3f, lostB, 1e-4f, "bola api menembus sampai musuh kedua");
        }

        [UnityTest]
        public IEnumerator Bounce0_DestroyedByWall()
        {
            yield return new EnterPlayMode();
            MakeWall(new Vector2(X + 3f, 0f));
            var fb = MakeFireball(new Vector2(X, 0f));
            fb.Launch(Vector2.right, Shot());
            yield return Steps(20);
            yield return null;
            bool gone = fb == null;
            yield return new ExitPlayMode();
            Assert.IsTrue(gone);
        }

        [UnityTest]
        public IEnumerator Bounce1_ComesBack()
        {
            yield return new EnterPlayMode();
            MakeWall(new Vector2(X + 3f, 0f));
            var fb = MakeFireball(new Vector2(X, 0f));
            fb.Launch(Vector2.right, Shot(bounce: 1, range: 30f));
            yield return Steps(15);
            yield return null;
            bool alive = fb != null;
            float vx = alive ? fb.GetComponent<Rigidbody2D>().linearVelocity.x : 0f;
            yield return new ExitPlayMode();
            Assert.IsTrue(alive, "pantulan pertama tidak menghancurkan bola api");
            Assert.Less(vx, 0f, "bola api berbalik arah setelah memantul");
        }

        // Review Focus 1: cahaya bola api sendiri menerangi musuh B yang dekat; homing tetap harus memilih A,
        // yang terang karena sumber cahaya lain.
        [UnityTest]
        public IEnumerator Homing_TurnsTowardLitEnemy_NotDarkOne()
        {
            yield return new EnterPlayMode();
            var lamp = new TestLight { Radius = 2f };
            float lostA, lostB;
            try
            {
                var a = MakeEnemy(new Vector2(X + 4f, 2f), 100f);
                var b = MakeEnemy(new Vector2(X + 2f, -1f), 100f);
                lamp.Position = a.transform.position;
                LightFieldHost.Field.Register(lamp);
                yield return Steps(2);
                MakeFireball(new Vector2(X, 0f), withLight: true).Launch(Vector2.right, Shot(homing: 360f));
                yield return Steps(25);
                lostA = 100f - a.CurrentHp;
                lostB = 100f - b.CurrentHp;
            }
            finally { LightFieldHost.Field.Unregister(lamp); }
            yield return new ExitPlayMode();
            Assert.Greater(lostA, 0f, "homing berbelok ke musuh terang");
            Assert.AreEqual(0f, lostB, "musuh gelap tidak dikejar walau diterangi bola api itu sendiri");
        }
        static RelicDefinition Relic(Stat stat, StatOp op, float value, bool healFull = false)
        {
            var r = ScriptableObject.CreateInstance<RelicDefinition>();
            r.mods = new[] { new StatMod { stat = stat, op = op, value = value } };
            r.healFull = healFull;
            return r;
        }

        // Pemain uji di x ≥ 100. Field serial diisi sebelum Awake.
        internal static PlayerController MakePlayer(Fireball fireballPrefab = null, LightSourceBehaviour light = null)
        {
            var go = new GameObject("TestPlayer");
            go.SetActive(false);
            go.transform.position = new Vector2(X, 10f);
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<CircleCollider2D>().radius = 0.35f;
            var pc = go.AddComponent<PlayerController>();
            var so = new SerializedObject(pc);
            if (fireballPrefab != null) so.FindProperty("fireballPrefab").objectReferenceValue = fireballPrefab;
            if (light != null)
            {
                light.transform.SetParent(go.transform, false);
                so.FindProperty("lightSource").objectReferenceValue = light;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            return pc;
        }

        [UnityTest]
        public IEnumerator SplitRelic_SpawnsTwoFireballs()
        {
            yield return new EnterPlayMode();
            var template = MakeFireball(new Vector2(X, 20f));
            template.gameObject.SetActive(false);
            var pc = MakePlayer(template);
            int before = Object.FindObjectsByType<Fireball>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
            pc.AddRelic(Relic(Stat.ProjectileCount, StatOp.Add, 1f));
            pc.Shoot(Vector2.right);
            int spawned = Object.FindObjectsByType<Fireball>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length - before;
            yield return new ExitPlayMode();
            Assert.AreEqual(2, spawned, "Bara Kembar: satu tembakan mengeluarkan 2 bola api");
        }

        [UnityTest]
        public IEnumerator HeartRelic_RaisesMaxAndHeals()
        {
            yield return new EnterPlayMode();
            var pc = MakePlayer();
            pc.TakeHit(new HitInfo(2f, Element.None, false, Vector2.zero));
            pc.AddRelic(Relic(Stat.MaxHp, StatOp.Add, 2f, healFull: true));
            float max = pc.Health.Max, cur = pc.Health.Current;
            yield return new ExitPlayMode();
            Assert.AreEqual(8f, max);
            Assert.AreEqual(8f, cur);
        }

        [UnityTest]
        public IEnumerator LanternRelic_GrowsPlayerLight()
        {
            yield return new EnterPlayMode();
            var light = new GameObject("PlayerLight").AddComponent<LightSourceBehaviour>();
            var pc = MakePlayer(light: light);
            pc.AddRelic(Relic(Stat.LightRadius, StatOp.Percent, 0.25f));
            float r = light.Radius;
            yield return new ExitPlayMode();
            Assert.AreEqual(3.75f, r, 1e-4f, "Lentera Tulang: radius cahaya +25%");
        }
        static BlazeRing MakeBlaze(Vector2 pos)
        {
            var go = new GameObject("Blaze");
            go.SetActive(false);
            go.transform.position = pos;
            var light = new GameObject("Light");
            light.transform.SetParent(go.transform, false);
            light.SetActive(false);
            var src = light.AddComponent<LightSourceBehaviour>();
            var ring = go.AddComponent<BlazeRing>();
            var so = new SerializedObject(ring);
            so.FindProperty("lightSource").objectReferenceValue = src;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            return ring;
        }

        // Kobaran (GDD §4.1): radius 2,5 tile, damage 6. Cahayanya menyala sebelum damage, jadi musuh di dalam
        // ring Tersingkap: 6 × 1,25 = 7,5.
        [UnityTest]
        public IEnumerator Blaze_DamagesEnemiesInRadius_WithLitBonus()
        {
            yield return new EnterPlayMode();
            var near = MakeEnemy(new Vector2(X + 2f, 30f), 100f);
            var far = MakeEnemy(new Vector2(X + 3f, 30f), 100f);
            yield return Steps(2);
            MakeBlaze(new Vector2(X, 30f)).Init(6f, 2.5f, 2f, 1f);
            float lostNear = 100f - near.CurrentHp, lostFar = 100f - far.CurrentHp;
            yield return new ExitPlayMode();
            Assert.AreEqual(7.5f, lostNear, 1e-4f);
            Assert.AreEqual(0f, lostFar);
        }

        [UnityTest]
        public IEnumerator Blaze_TriggersFrostPuddle()
        {
            yield return new EnterPlayMode();
            var puddle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runecrypt/Prefabs/FrostPuddle.prefab");
            Object.Instantiate(puddle, new Vector3(X + 1f, 40f, 0f), Quaternion.identity);
            yield return Steps(2);
            int before = Object.FindObjectsByType<SteamCloud>(FindObjectsSortMode.None).Length;
            MakeBlaze(new Vector2(X, 40f)).Init(6f, 2.5f, 2f, 1f);
            int clouds = Object.FindObjectsByType<SteamCloud>(FindObjectsSortMode.None).Length - before;
            yield return new ExitPlayMode();
            Assert.AreEqual(1, clouds, "Kobaran dihitung sebagai api, jadi memicu Uap tepat sekali");
        }
    }
}
