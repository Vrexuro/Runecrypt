using System.Collections;
using NUnit.Framework;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runecrypt.Tests
{
    // Review Focus #2: pemain mati di tengah hit-stop, layar Tumbang harus tetap berhenti total.
    public class HitStopPlayModeTests
    {
        [UnityTest]
        public IEnumerator PauseDuringHitStop_StaysFrozen()
        {
            yield return new EnterPlayMode();
            var hs = new GameObject("HitStop").AddComponent<HitStop>();
            hs.Request(0.08f);
            yield return null;
            GamePause.IsPaused = true; Time.timeScale = 0f;              // pemain mati di tengah hit-stop
            yield return new WaitForSecondsRealtime(0.2f);
            float scale = Time.timeScale;
            GamePause.IsPaused = false; Time.timeScale = 1f;
            yield return new ExitPlayMode();
            Assert.AreEqual(0f, scale);
        }
    }
}
