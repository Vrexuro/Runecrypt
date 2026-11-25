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
    // M3a (Docs/plans/2026-11-18-m3a-musuh-es.md): muatan elemen musuh, Biarawan Beku, Kristal Es.
    // Scene test kosong: objek dibuat sendiri di x >= 100 (memori unity-playmode-test-gotchas).
    public class EnemyReactionPlayModeTests
    {
        const float X = 100f;
        const string Uap = "Assets/Runecrypt/Data/Reactions/Uap.asset";

        static Enemy MakeElemental(Vector2 pos, float hp, Element element)
        {
            var enemy = ProjectilePlayModeTests.MakeEnemy(pos, hp);
            enemy.Definition.element = element;
            var so = new SerializedObject(enemy);
            var book = so.FindProperty("reactionBook");
            book.arraySize = 1;
            book.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ReactionDefinition>(Uap);
            so.ApplyModifiedPropertiesWithoutUndo();
            return enemy;
        }

        static HitInfo Fire(Enemy e, float damage = 3f) => new HitInfo(damage, Element.Fire, false, (Vector2)e.transform.position + Vector2.left);

        static int Clouds() => Object.FindObjectsByType<SteamCloud>(FindObjectsSortMode.None).Length;

        static IEnumerator WaitGameSeconds(float seconds)
        {
            float target = Time.time + seconds;
            float giveUp = Time.realtimeSinceStartup + seconds * 3f + 2f;
            while (Time.time < target && Time.realtimeSinceStartup < giveUp) yield return null;
        }

        // ---------- Task 2: muatan elemen ----------
        [UnityTest]
        public IEnumerator IceEnemy_FireHit_SpawnsSteamOnce_ThenRecharges()
        {
            yield return new EnterPlayMode();
            var monk = MakeElemental(new Vector2(X, 0f), 500f, Element.Ice);
            monk.TakeHit(Fire(monk));
            int afterFirst = Clouds();
            yield return WaitGameSeconds(1f);
            monk.TakeHit(Fire(monk));
            int afterSecond = Clouds();
            yield return WaitGameSeconds(3.2f); // total > 4 dtk: muatan terisi, awan pertama (3 dtk) sudah hilang
            int beforeThird = Clouds();
            monk.TakeHit(Fire(monk));
            int afterThird = Clouds();
            yield return new ExitPlayMode();
            Assert.AreEqual(1, afterFirst, "tembakan pertama memicu Uap");
            Assert.AreEqual(1, afterSecond, "selama muatan kosong tidak ada Uap baru");
            Assert.AreEqual(0, beforeThird);
            Assert.AreEqual(1, afterThird, "setelah 4 dtk muatan terisi lagi");
        }

        [UnityTest]
        public IEnumerator IceEnemy_FireHitDuringEmptyCharge_StillDamages()
        {
            yield return new EnterPlayMode();
            var monk = MakeElemental(new Vector2(X, 0f), 500f, Element.Ice);
            monk.TakeHit(Fire(monk));
            float before = monk.CurrentHp;
            monk.TakeHit(Fire(monk));
            float delta = before - monk.CurrentHp;
            yield return new ExitPlayMode();
            // Uap adalah sumber cahaya (GDD §5.5), jadi musuh di dalamnya Tersingkap: 3 x 1,25.
            Assert.GreaterOrEqual(delta, 3f, "damage api tetap masuk walau muatan kosong");
        }

        [UnityTest]
        public IEnumerator IceEnemy_KillingBlow_StillTriggersReaction()
        {
            yield return new EnterPlayMode();
            var monk = MakeElemental(new Vector2(X, 0f), 3f, Element.Ice);
            monk.TakeHit(Fire(monk, 10f));
            int clouds = Clouds();
            yield return new ExitPlayMode();
            Assert.AreEqual(1, clouds);
        }

        [UnityTest]
        public IEnumerator PlainEnemy_FireHit_NoReaction()
        {
            yield return new EnterPlayMode();
            var bones = MakeElemental(new Vector2(X, 0f), 500f, Element.None);
            bones.TakeHit(Fire(bones));
            int clouds = Clouds();
            yield return new ExitPlayMode();
            Assert.AreEqual(0, clouds);
        }

        // ---------- Task 4: Biarawan Beku dan serpih es ----------
        static EnemyShot ShotTemplate()
        {
            var go = new GameObject("ShotTemplate");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.15f;
            return go.AddComponent<EnemyShot>();
        }

        static RangedBrain AddRanged(Enemy e, EnemyShot template)
        {
            var brain = e.gameObject.AddComponent<RangedBrain>();
            var so = new SerializedObject(brain);
            so.FindProperty("shotPrefab").objectReferenceValue = template;
            so.ApplyModifiedPropertiesWithoutUndo();
            return brain;
        }

        static EnemyShot[] Shots() => Object.FindObjectsByType<EnemyShot>(FindObjectsSortMode.None);

        static IEnumerator WaitUntil(System.Func<bool> done, float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!done() && Time.realtimeSinceStartup < end) yield return null;
        }

        [UnityTest]
        public IEnumerator RangedEnemy_FiresThreeShardsAtPlayer()
        {
            yield return new EnterPlayMode();
            var player = CryptKitPlayModeTests.MakePlayer(new Vector2(X, 0f));
            var monk = MakeElemental(new Vector2(X + 5f, 0f), 500f, Element.Ice);
            AddRanged(monk, ShotTemplate());
            yield return WaitUntil(() => Shots().Length >= 3, 5f);
            var shots = Shots();
            int count = shots.Length;
            bool allTowardPlayer = shots.All(s => Vector2.Dot(s.GetComponent<Rigidbody2D>().linearVelocity.normalized, Vector2.left) > 0.9f);
            yield return new ExitPlayMode();
            Assert.AreEqual(3, count, "3 serpih per tembakan");
            Assert.IsTrue(allTowardPlayer, "sebaran 15 derajat ke arah pemain");
        }

        [UnityTest]
        public IEnumerator RangedEnemy_BlindOrStunned_DoesNotFire()
        {
            yield return new EnterPlayMode();
            CryptKitPlayModeTests.MakePlayer(new Vector2(X, 0f));
            var template = ShotTemplate();
            var blind = MakeElemental(new Vector2(X + 5f, 0f), 500f, Element.Ice);
            var stunned = MakeElemental(new Vector2(X, 5f), 500f, Element.Ice);
            blind.Blind(Time.time + 10f);
            stunned.Stun(Time.time + 10f);
            AddRanged(blind, template);
            AddRanged(stunned, template);
            yield return WaitGameSeconds(3f); // lebih lama dari jeda tembak 2,2 dtk
            int shots = Shots().Length;
            yield return new ExitPlayMode();
            Assert.AreEqual(0, shots);
        }

        [UnityTest]
        public IEnumerator EnemyShot_DamagesPlayer_IgnoresEnemies_DiesOnWall()
        {
            yield return new EnterPlayMode();
            var template = ShotTemplate();
            EnemyShot Fire(Vector2 at)
            {
                var s = Object.Instantiate(template, at, Quaternion.identity);
                s.gameObject.SetActive(true);
                s.Launch(Vector2.right);
                return s;
            }
            var player = CryptKitPlayModeTests.MakePlayer(new Vector2(X, 0f));
            float hpBefore = player.Health.Current;
            var atPlayer = Fire(new Vector2(X - 2f, 0f));
            var enemy = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 20f, 10f), 50f);
            var pastEnemy = Fire(new Vector2(X + 18f, 10f));
            var wall = new GameObject("Wall") { layer = LayerMask.NameToLayer("Walls") };
            wall.transform.position = new Vector2(X + 40f, 20f);
            wall.AddComponent<BoxCollider2D>().size = new Vector2(1f, 4f);
            var atWall = Fire(new Vector2(X + 38f, 20f));
            yield return WaitGameSeconds(0.7f);
            bool playerShotGone = atPlayer == null, wallShotGone = atWall == null, enemyShotAlive = pastEnemy != null;
            float hpAfter = player.Health.Current, enemyHp = enemy.CurrentHp;
            yield return new ExitPlayMode();
            Assert.AreEqual(hpBefore - 1f, hpAfter, 1e-3f, "serpih: damage 1 ke pemain");
            Assert.IsTrue(playerShotGone, "serpih hancur saat mengenai pemain");
            Assert.AreEqual(50f, enemyHp, 1e-3f, "serpih tidak melukai musuh");
            Assert.IsTrue(enemyShotAlive, "serpih menembus musuh");
            Assert.IsTrue(wallShotGone, "serpih hancur di dinding");
        }

        [UnityTest]
        public IEnumerator IceMonk_Death_LeavesFrostPuddle()
        {
            yield return new EnterPlayMode();
            var monk = MakeElemental(new Vector2(X, 0f), 3f, Element.Ice);
            monk.Definition.dropOnDeath = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runecrypt/Prefabs/FrostPuddle.prefab");
            monk.TakeHit(Fire(monk, 10f));
            int puddles = Object.FindObjectsByType<ReactiveProp>(FindObjectsSortMode.None).Length;
            yield return new ExitPlayMode();
            Assert.AreEqual(1, puddles);
        }

        // Review Focus 4: genangan yang dijatuhkan tidak ikut bereaksi di frame pukulan terakhir.
        [UnityTest]
        public IEnumerator IceMonk_DroppedPuddle_DoesNotReactSameFrame()
        {
            yield return new EnterPlayMode();
            var monk = MakeElemental(new Vector2(X, 0f), 3f, Element.Ice);
            monk.Definition.dropOnDeath = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runecrypt/Prefabs/FrostPuddle.prefab");
            monk.TakeHit(Fire(monk, 10f));
            yield return ProjectilePlayModeTests.Steps(2);
            int clouds = Clouds();
            var puddle = Object.FindFirstObjectByType<ReactiveProp>();
            bool puddleReady = puddle != null && puddle.GetComponent<SpriteRenderer>().enabled;
            yield return new ExitPlayMode();
            Assert.AreEqual(1, clouds, "hanya Uap dari Biarawan");
            Assert.IsTrue(puddleReady, "genangan masih utuh dan siap dipicu tembakan berikutnya");
        }

        // ---------- Task 6: Kristal Es dan Mahkota Asap ----------
        static ReactiveProp MakeCrystal(Vector2 pos)
        {
            var go = new GameObject("IceCrystal");
            go.transform.position = pos;
            go.AddComponent<SpriteRenderer>();
            go.AddComponent<BoxCollider2D>().size = new Vector2(0.8f, 0.8f);
            var prop = go.AddComponent<ReactiveProp>();
            var so = new SerializedObject(prop);
            var book = so.FindProperty("reactionBook");
            book.arraySize = 1;
            book.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ReactionDefinition>(Uap);
            so.FindProperty("singleUse").boolValue = true;
            so.FindProperty("shatterDamage").floatValue = 4f;
            so.FindProperty("shatterRadius").floatValue = 1.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return prop;
        }

        static HitInfo FireAt(Vector2 p) => new HitInfo(3f, Element.Fire, false, p + Vector2.left);

        [UnityTest]
        public IEnumerator IceCrystal_FireHit_SpawnsSteamOnce_AndBreaks()
        {
            yield return new EnterPlayMode();
            var crystal = MakeCrystal(new Vector2(X, 0f));
            crystal.TakeHit(FireAt(crystal.transform.position));
            crystal.TakeHit(FireAt(crystal.transform.position)); // tembakan kedua di frame yang sama
            int clouds = Clouds();
            yield return null;
            bool broken = crystal == null;
            yield return new ExitPlayMode();
            Assert.AreEqual(1, clouds, "Uap sekali");
            Assert.IsTrue(broken, "Kristal Es pecah setelah bereaksi");
        }

        // Keputusan 25 Nov (GDD §5.1): objek elemen juga melukai musuh di dekatnya; pemain tidak (reaksi tidak melukai pemain).
        [UnityTest]
        public IEnumerator IceCrystal_Shatter_DamagesNearbyEnemy_NotPlayer()
        {
            yield return new EnterPlayMode();
            var crystal = MakeCrystal(new Vector2(X, 0f));
            var near = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 1f, 0f), 50f);
            var far = ProjectilePlayModeTests.MakeEnemy(new Vector2(X + 3f, 0f), 50f);
            var player = CryptKitPlayModeTests.MakePlayer(new Vector2(X - 1f, 0f));
            float playerHp = player.Health.Current;
            crystal.TakeHit(FireAt(crystal.transform.position));
            float nearHp = near.CurrentHp, farHp = far.CurrentHp, playerAfter = player.Health.Current;
            yield return new ExitPlayMode();
            Assert.LessOrEqual(nearHp, 46f, "musuh dalam 1,5 tile kena pecahan (4, +25% kalau Tersingkap)");
            Assert.AreEqual(50f, farHp, 1e-3f, "musuh di 3 tile utuh");
            Assert.AreEqual(playerHp, playerAfter, 1e-3f, "pecahan tidak melukai pemain");
        }

        [UnityTest]
        public IEnumerator SmokeCrown_SteamRadiusAndDurationPlusHalf()
        {
            yield return new EnterPlayMode();
            var player = CryptKitPlayModeTests.MakePlayer(new Vector2(X, 0f));
            player.Stats.Add(new[]
            {
                new StatMod { stat = Stat.SteamRadius, op = StatOp.Percent, value = 0.5f },
                new StatMod { stat = Stat.SteamDuration, op = StatOp.Percent, value = 0.5f },
            });
            var cloud = ReactionSpawner.Spawn(AssetDatabase.LoadAssetAtPath<ReactionDefinition>(Uap), new Vector2(X + 10f, 0f))
                .GetComponent<SteamCloud>();
            float radius = cloud.Radius, duration = cloud.Duration;
            yield return new ExitPlayMode();
            Assert.AreEqual(3.75f, radius, 1e-3f);
            Assert.AreEqual(4.5f, duration, 1e-3f);
        }

        // ---------- Task 8: efek mati sekali putar ----------
        [UnityTest]
        public IEnumerator SpriteFlipbook_PlayOnce_DestroysAfterLastFrame()
        {
            yield return new EnterPlayMode();
            var tex = new Texture2D(2, 2);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            var go = new GameObject("DeathFx");
            go.SetActive(false);
            go.AddComponent<SpriteRenderer>();
            var fb = go.AddComponent<SpriteFlipbook>();
            var so = new SerializedObject(fb);
            var frames = so.FindProperty("frames");
            frames.arraySize = 3;
            for (int i = 0; i < 3; i++) frames.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
            so.FindProperty("fps").floatValue = 10f;
            so.FindProperty("playOnce").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            yield return WaitGameSeconds(0.15f);
            bool aliveMid = go != null;
            yield return WaitGameSeconds(0.4f);
            bool goneAfter = go == null;
            yield return new ExitPlayMode();
            Assert.IsTrue(aliveMid, "masih tampil di tengah animasi");
            Assert.IsTrue(goneAfter, "hilang setelah frame terakhir (3 frame / 10 fps = 0,3 dtk)");
        }
    }
}
