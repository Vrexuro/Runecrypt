using NUnit.Framework;
using Runecrypt.Game;
using UnityEngine;

namespace Runecrypt.Tests
{
    // Komponen visual M1.5 (Review Focus #3 dan #4).
    public class FeedbackComponentTests
    {
        [Test]
        public void Apply_OverridesExternalColor()
        {
            var parent = new GameObject("P").AddComponent<SpriteRenderer>();
            var flash = new GameObject("F").AddComponent<SpriteRenderer>().gameObject.AddComponent<HitFlash>();
            flash.SetSource(parent);
            var sr = flash.GetComponent<SpriteRenderer>();
            sr.color = Color.white;                        // seperti fade spawn ArenaDirector
            flash.Apply(10f);
            Assert.AreEqual(0f, sr.color.a);
            flash.Flash(Color.white, 0.35f, 0.08f); flash.Apply(Time.time);
            Assert.AreEqual(0.35f, sr.color.a, 1e-4);
            Object.DestroyImmediate(parent.gameObject); Object.DestroyImmediate(flash.gameObject);
        }

        // M2 (GDD §5.5 aturan 1): garis tepi jingga mengikuti sprite musuh dan hanya tampil saat Tersingkap.
        [Test]
        public void ExposedOutline_CopiesSpriteAndHidesWhenNotVisible()
        {
            var tex = new Texture2D(4, 4);
            var sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16);
            var parent = new GameObject("P").AddComponent<SpriteRenderer>();
            parent.sprite = sprite; parent.flipX = true; parent.sortingOrder = 5;
            var outline = new GameObject("O").AddComponent<SpriteRenderer>().gameObject.AddComponent<ExposedOutline>();
            outline.SetSource(parent);
            var sr = outline.GetComponent<SpriteRenderer>();

            outline.SetVisible(true); outline.Apply();
            Assert.AreSame(sprite, sr.sprite);
            Assert.IsTrue(sr.flipX);
            Assert.AreEqual(4, sr.sortingOrder, "di belakang tubuh musuh");
            Assert.IsTrue(sr.enabled);

            outline.SetVisible(false); outline.Apply();
            Assert.IsFalse(sr.enabled, "musuh di gelap hanya terlihat matanya");
            Object.DestroyImmediate(parent.gameObject); Object.DestroyImmediate(outline.gameObject);
            Object.DestroyImmediate(sprite); Object.DestroyImmediate(tex);
        }

        [Test]
        public void Registry_CapDropsOldest()
        {
            RemainsRegistry.Clear(); RemainsRegistry.Capacity = 2;
            var a = new GameObject("a"); var b = new GameObject("b"); var c = new GameObject("c");
            RemainsRegistry.Add(a); RemainsRegistry.Add(b); RemainsRegistry.Add(c);
            Assert.AreEqual(2, RemainsRegistry.Count);
            Assert.IsTrue(a == null, "yang tertua dihancurkan");
            RemainsRegistry.Capacity = 30; Object.DestroyImmediate(b); Object.DestroyImmediate(c); RemainsRegistry.Clear();
        }

        [Test]
        public void DestroyedEntries_AreSkipped()
        {
            RemainsRegistry.Clear(); var a = new GameObject("a"); RemainsRegistry.Add(a);
            Object.DestroyImmediate(a); var b = new GameObject("b"); RemainsRegistry.Add(b);
            Assert.AreEqual(1, RemainsRegistry.Count);
            Object.DestroyImmediate(b); RemainsRegistry.Clear();
        }
    }
}
