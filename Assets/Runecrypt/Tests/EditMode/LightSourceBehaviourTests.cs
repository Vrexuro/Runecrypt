using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Tests
{
    public class LightSourceBehaviourTests
    {
        GameObject go;
        [TearDown] public void TearDown() => Object.DestroyImmediate(go);

        [Test]
        public void SettingRadius_UpdatesLight2D()
        {
            go = new GameObject("light");
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            var src = go.AddComponent<LightSourceBehaviour>();

            src.Radius = 4.5f;

            Assert.AreEqual(4.5f, light.pointLightOuterRadius, 1e-4);
            Assert.AreEqual(4.5f, ((ILightSource)src).Radius, 1e-4);
        }

        [Test]
        public void Position_FollowsTransform()
        {
            go = new GameObject("light");
            go.AddComponent<Light2D>();
            var src = go.AddComponent<LightSourceBehaviour>();
            go.transform.position = new Vector3(2, 3, 0);
            Assert.AreEqual(new Vector2(2, 3), ((ILightSource)src).Position);
        }
    }
}
