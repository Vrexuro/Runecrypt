using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Satu per scene. Menyediakan LightField dengan pengecekan dinding lewat Physics2D.
    [DefaultExecutionOrder(-1000)]
    public class LightFieldHost : MonoBehaviour
    {
        [SerializeField] LayerMask wallMask;

        static int mask;
        static LightField field;

        // Lazy supaya sumber cahaya yang OnEnable lebih dulu dari Awake host tetap terdaftar.
        public static LightField Field => field ??= new LightField(IsBlocked);

        void Awake() => mask = wallMask;

        static bool IsBlocked(Vector2 from, Vector2 to) =>
            mask != 0 && Physics2D.Linecast(from, to, mask).collider != null;
    }
}
