using NUnit.Framework;
using UnityEngine;

namespace Runecrypt.Tests
{
    // M2.5 Isi Makam (Docs/specs/2026-11-24-isi-makam-design.md).
    public class CryptKitTests
    {
        static int L(string name) => LayerMask.NameToLayer(name);

        [Test]
        public void PropsLayer_Exists() => Assert.GreaterOrEqual(L("Props"), 0);

        // Anglo dan Guci padat bagi pemain dan musuh, dan bisa dikenai tembakan; uap tidak menyentuhnya.
        [Test]
        public void PropsLayer_CollidesWithActorsNotReactives()
        {
            int props = L("Props");
            Assert.GreaterOrEqual(props, 0);
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Player")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Enemies")));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(props, L("Projectiles")));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(props, L("Reactives")));
        }
    }
}
