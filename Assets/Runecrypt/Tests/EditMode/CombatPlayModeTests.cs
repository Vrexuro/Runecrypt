using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // Perilaku yang bergantung pada callback fisika. Dijalankan di Play mode dengan fisika
    // dilangkahkan manual (Physics2D.Simulate) supaya deterministik walau editor tidak fokus.
    public class CombatPlayModeTests
    {
        static EnemyDefinition Def(float hp)
        {
            var d = ScriptableObject.CreateInstance<EnemyDefinition>();
            d.maxHp = hp; d.moveSpeed = 0f; d.contactDamage = 1f; d.contactInterval = 0.1f;
            return d;
        }

        static Enemy MakeEnemy(Vector2 pos, EnemyDefinition def, float gravity = 0f)
        {
            var go = new GameObject("TestEnemy");
            go.SetActive(false);
            go.transform.position = pos;
            go.AddComponent<Rigidbody2D>().gravityScale = gravity;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var enemy = go.AddComponent<Enemy>();
            var so = new SerializedObject(enemy);
            so.FindProperty("definition").objectReferenceValue = def;
            so.FindProperty("lightRules").objectReferenceValue = ScriptableObject.CreateInstance<LightRules>();
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);
            return enemy;
        }

        static void Step(int steps)
        {
            for (int i = 0; i < steps; i++) Physics2D.Simulate(0.02f);
        }

        [UnityTest]
        public IEnumerator BlindEnemy_DoesNotDamagePlayer()
        {
            yield return new EnterPlayMode();
            var mode = Physics2D.simulationMode;
            float hp;
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                new GameObject("Cam") { tag = "MainCamera" }.AddComponent<Camera>();
                var playerGo = new GameObject("Player");
                playerGo.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                playerGo.AddComponent<CircleCollider2D>().radius = 0.35f;
                var player = playerGo.AddComponent<PlayerController>();

                // Musuh jatuh (gravitasi) ke atas pemain supaya kontaknya terus berlangsung.
                var enemy = MakeEnemy(new Vector2(0f, 0.8f), Def(12f), gravity: 1f);
                enemy.Blind(Time.time + 10f);
                Step(30);
                hp = player.Health.Current;
            }
            finally
            {
                Physics2D.simulationMode = mode;
            }
            yield return new ExitPlayMode();

            Assert.AreEqual(6f, hp, "musuh yang buta (Uap) tidak boleh menyerang (GDD §5.4)");
        }

        [UnityTest]
        public IEnumerator Fireball_HitsOnlyOneTarget()
        {
            yield return new EnterPlayMode();
            var mode = Physics2D.simulationMode;
            int deaths = 0;
            System.Action<Enemy> onDied = _ => deaths++;
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                Enemy.AnyDied += onDied;

                // Dua musuh berdempetan di jalur bola api; HP 3 = mati oleh satu tembakan.
                MakeEnemy(new Vector2(3f, 0.3f), Def(3f));
                MakeEnemy(new Vector2(3f, -0.3f), Def(3f));
                Step(5);

                var fbGo = new GameObject("Fireball");
                fbGo.SetActive(false);
                fbGo.AddComponent<Rigidbody2D>().gravityScale = 0f;
                var col = fbGo.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.2f;
                var fb = fbGo.AddComponent<Fireball>();
                fbGo.SetActive(true);
                fb.Launch(Vector2.right);
                Step(30);
            }
            finally
            {
                Enemy.AnyDied -= onDied;
                Physics2D.simulationMode = mode;
            }
            yield return new ExitPlayMode();

            Assert.AreEqual(1, deaths, "satu bola api hanya boleh mengenai satu sasaran");
        }
    }
}
