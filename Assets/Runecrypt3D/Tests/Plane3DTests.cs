using System.Collections;
using NUnit.Framework;
using Runecrypt.Game3D;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests3D
{
    public class Plane3DTests
    {
        [Test]
        public void ToWorld_MapsYToZ()
        {
            Assert.AreEqual(new Vector3(2f, 0f, 3f), Plane3D.ToWorld(new Vector2(2f, 3f)));
            Assert.AreEqual(new Vector2(2f, 3f), Plane3D.ToPlane(new Vector3(2f, 9f, 3f)));
        }

        [Test]
        public void RayToFloor_HitsGround()
        {
            bool hit = Plane3D.RayToFloor(new Ray(new Vector3(0f, 5f, -5f), new Vector3(0f, -1f, 1f)), 0f, out var p);
            Assert.IsTrue(hit);
            Assert.AreEqual(0f, p.x, 1e-4f);
            Assert.AreEqual(0f, p.y, 1e-4f);
        }

        // Review Focus 1: kursor mengarah ke langit tidak boleh menghasilkan NaN.
        [Test]
        public void RayToFloor_PointingUp_ReturnsFalse()
        {
            Assert.IsFalse(Plane3D.RayToFloor(new Ray(new Vector3(0f, 5f, 0f), Vector3.up), 0f, out _));
        }

        [Test]
        public void RayToFloor_Parallel_ReturnsFalse()
        {
            Assert.IsFalse(Plane3D.RayToFloor(new Ray(new Vector3(0f, 5f, 0f), Vector3.forward), 0f, out _));
        }

        // Review Focus 2: cahaya tidak menembus dinding.
        [UnityTest]
        public IEnumerator Light3D_WallBlocksExposure()
        {
            yield return new EnterPlayMode();
            var lightGo = new GameObject("L");
            lightGo.transform.position = new Vector3(100f, 0.5f, 0f);
            lightGo.AddComponent<LightSource3D>().Radius = 3f;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.layer = LayerMask.NameToLayer("Walls");
            wall.transform.position = new Vector3(101f, 0.5f, 0f);
            wall.transform.localScale = new Vector3(0.2f, 1f, 2f);
            Physics.SyncTransforms();
            yield return null;

            Assert.IsTrue(LightFieldHost3D.Field.IsLit(new Vector2(99f, 0f)), "sisi yang sama dengan cahaya terang");
            Assert.IsFalse(LightFieldHost3D.Field.IsLit(new Vector2(102f, 0f)), "di balik dinding tetap gelap");

            Object.Destroy(lightGo);
            Object.Destroy(wall);
            yield return new ExitPlayMode();
        }
    }
}
