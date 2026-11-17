using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // Temuan review akhir M1.5 yang hanya terlihat dengan frame sungguhan.
    public class FeedbackPlayModeTests
    {
        // Review Important #3: pukulan terakhir harus sempat menampilkan kedipan (spec §5.1 langkah 1),
        // jadi tubuh musuh bertahan sesaat dan tidak lagi bisa disentuh.
        [UnityTest]
        public IEnumerator KillingBlow_LeavesBodyBrieflyForFlash()
        {
            yield return new EnterPlayMode();
            var def = ScriptableObject.CreateInstance<EnemyDefinition>();
            def.maxHp = 3f; def.moveSpeed = 0f;
            var go = new GameObject("TestEnemy");
            go.SetActive(false);
            go.AddComponent<Rigidbody2D>().gravityScale = 0f;
            var col = go.AddComponent<CircleCollider2D>();
            var enemy = go.AddComponent<Enemy>();
            var so = new SerializedObject(enemy);
            so.FindProperty("definition").objectReferenceValue = def;
            so.FindProperty("lightRules").objectReferenceValue = ScriptableObject.CreateInstance<LightRules>();
            so.ApplyModifiedPropertiesWithoutUndo();
            go.SetActive(true);

            enemy.TakeHit(new HitInfo(3f, Element.Fire, false, Vector2.left));
            yield return null;
            bool stillThere = go != null;
            bool colliderOff = stillThere && !col.enabled;
            yield return new WaitForSeconds(0.3f);
            bool goneLater = go == null;
            yield return new ExitPlayMode();

            Assert.IsTrue(stillThere, "tubuh harus bertahan sesaat untuk kedipan pukulan terakhir");
            Assert.IsTrue(colliderOff, "tubuh yang sedang hilang tidak boleh lagi melukai atau ditembak");
            Assert.IsTrue(goneLater, "tubuh tetap hilang tak lama kemudian");
        }

        // Review "declined to judge" → ruling: FeedbackPlayer.Awake bisa jalan sebelum kamera terdaftar,
        // sehingga guncangan diam-diam tidak pernah aktif. Kamera harus dicari lagi saat dibutuhkan.
        // Memakai trauma (bertahan beberapa frame), bukan sentakan satu frame, supaya tidak bergantung pada
        // urutan coroutine test terhadap LateUpdate.
        [UnityTest]
        public IEnumerator Shake_FindsCameraCreatedAfterFeedbackPlayer()
        {
            yield return new EnterPlayMode();
            // EnterPlayMode memainkan scene yang sedang terbuka; kamera scene akan menang sebagai Camera.main.
            foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) Object.Destroy(c.gameObject);
            yield return null;
            new GameObject("Feedback").AddComponent<FeedbackPlayer>();   // Awake: belum ada kamera
            var cam = new GameObject("Cam") { tag = "MainCamera" };
            cam.AddComponent<Camera>();
            cam.AddComponent<CameraShake>();
            yield return null;
            var profile = ScriptableObject.CreateInstance<FeedbackProfile>();
            profile.shakeTrauma = 1f;
            FeedbackPlayer.Play(profile, Vector2.zero);
            float maxOffset = 0f;
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                maxOffset = Mathf.Max(maxOffset, cam.transform.position.magnitude);
            }
            yield return new ExitPlayMode();

            Assert.Greater(maxOffset, 0f, "guncangan harus sampai ke kamera");
        }
    }
}
