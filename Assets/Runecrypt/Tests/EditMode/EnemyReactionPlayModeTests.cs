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
    }
}
