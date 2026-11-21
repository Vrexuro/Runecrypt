using NUnit.Framework;
using Runecrypt.Game3D;

namespace Runecrypt.Tests3D
{
    // Laju klip Move = kecepatan / v0, dibatasi clamp; Pyromancer memutar terbalik saat mundur (spec animasi jalan).
    public class LocomotionMathTests
    {
        [TestCase(0f, 0f, 2f, .5f, 1.3f, false, .5f)]       // diam: batas bawah
        [TestCase(2f, 2f, 2f, .5f, 1.3f, false, 1f)]        // Tulang normal
        [TestCase(2.4f, 2.4f, 2f, .5f, 1.3f, false, 1.2f)]  // Tulang di gelap
        [TestCase(5f, 5f, 2f, .5f, 1.3f, false, 1.3f)]      // di atas clamp
        [TestCase(2f, -2f, 2f, .5f, 1.3f, false, 1f)]       // Tulang tidak pernah terbalik
        [TestCase(5f, -5f, 5f, .6f, 1.2f, true, -1f)]       // Pyromancer mundur
        [TestCase(5f, 0f, 5f, .6f, 1.2f, true, 1f)]         // menyamping tetap maju
        [TestCase(5f, -1f, 5f, .6f, 1.2f, true, 1f)]        // serong sedikit ke belakang tetap maju
        public void SpeedMul_Cases(float speed, float fwd, float v0, float min, float max, bool rev, float expected)
            => Assert.AreEqual(expected, LocomotionMath.SpeedMul(speed, fwd, v0, min, max, rev), 1e-4f);
    }
}
