using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Arah kejar musuh. Kalau jalur ke pemain bebas, lurus seperti sebelumnya; kalau terhalang dinding, pilar,
    // nisan, sarkofagus, guci, atau Anglo, musuh mengikuti FlowField yang memutari penghalang.
    // Satu field dipakai bersama semua musuh dan dibangun ulang saat pemain pindah sel (paling cepat tiap 0,1 detik)
    // atau tiap 0,5 detik (guci yang pecah membuka jalan). Tidak butuh objek di scene.
    public static class EnemyNav
    {
        const int HalfW = 32, HalfH = 18;           // area di sekitar pemain; arena 30x17 selalu tercakup
        const float MinRebuild = 0.1f, MaxAge = 0.5f;
        const float CellProbe = 0.8f;               // sel terhalang kalau kotak 0,8 di tengahnya menyentuh penghalang
        const int LookAhead = 6;

        static readonly FlowField field = new FlowField();
        static readonly Collider2D[] overlap = new Collider2D[1];
        static readonly RaycastHit2D[] hits = new RaycastHit2D[1];
        static ContactFilter2D filter;
        static bool hasFilter, built;
        static float builtAt;
        static Vector2Int builtTarget;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { built = false; hasFilter = false; }

        static ContactFilter2D Filter
        {
            get
            {
                if (!hasFilter)
                {
                    filter = new ContactFilter2D { useTriggers = false };
                    filter.SetLayerMask(LayerMask.GetMask("Walls", "Props"));
                    hasFilter = true;
                }
                return filter;
            }
        }

        public static Vector2 Direction(Vector2 from, Vector2 target, float radius)
        {
            Vector2 to = target - from;
            if (to.sqrMagnitude < 1e-6f) return Vector2.zero;
            if (Clear(from, target, radius)) return to.normalized;

            Rebuild(Cell(target));
            // Ikuti jalur beberapa sel ke depan dan bidik sel terjauh yang terlihat, supaya geraknya tidak zig-zag.
            var cell = Cell(from);
            Vector2 aim = default;
            bool has = false;
            for (int i = 0; i < LookAhead && field.TryStep(cell, out var next); i++)
            {
                Vector2 p = Center(next);
                if (has && !Clear(from, p, radius)) break;
                aim = p;
                has = true;
                cell = next;
            }
            return has ? (aim - from).normalized : to.normalized;
        }

        static void Rebuild(Vector2Int target)
        {
            float now = Time.time;
            float age = now - builtAt;
            bool stale = !built || age < 0f || age >= MaxAge || (target != builtTarget && age >= MinRebuild);
            if (!stale) return;
            var area = new RectInt(target.x - HalfW, target.y - HalfH, 2 * HalfW + 1, 2 * HalfH + 1);
            var f = Filter;
            field.Build(area, target, c =>
                Physics2D.OverlapBox(Center(c), new Vector2(CellProbe, CellProbe), 0f, f, overlap) > 0);
            built = true;
            builtAt = now;
            builtTarget = target;
        }

        static bool Clear(Vector2 from, Vector2 to, float radius)
        {
            Vector2 d = to - from;
            float len = d.magnitude;
            if (len < 1e-4f) return true;
            return Physics2D.CircleCast(from, radius * 0.9f, d / len, Filter, hits, len) == 0;
        }

        static Vector2Int Cell(Vector2 p) => new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y));
        static Vector2 Center(Vector2Int c) => new Vector2(c.x + 0.5f, c.y + 0.5f);
    }
}
