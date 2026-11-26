using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runecrypt.Core
{
    // Peta jarak ke satu target di grid sel 1x1 (Dijkstra 8 arah: lurus 10, diagonal 14, tanpa memotong sudut).
    // Dipakai pengejaran musuh supaya bisa memutari pilar, nisan, dan dinding. Satu field dipakai bersama semua musuh.
    public sealed class FlowField
    {
        public const int Straight = 10, Diagonal = 14;

        static readonly Vector2Int[] Neighbours =
        {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
        };

        RectInt area;
        int[] dist = Array.Empty<int>();
        bool[] blocked = Array.Empty<bool>();
        readonly List<(int cost, int index)> heap = new List<(int, int)>();

        public RectInt Area => area;

        public void Build(RectInt area, Vector2Int target, Func<Vector2Int, bool> isBlocked)
        {
            this.area = area;
            int n = area.width * area.height;
            if (dist.Length < n) { dist = new int[n]; blocked = new bool[n]; }
            for (int i = 0; i < n; i++)
            {
                dist[i] = -1;
                blocked[i] = isBlocked(CellOf(i));
            }
            heap.Clear();
            if (!Contains(target)) return;

            // Target selalu jadi sumber, walau selnya terhalang (pemain yang menempel ke nisan).
            int t = IndexOf(target);
            dist[t] = 0;
            Push(0, t);
            while (heap.Count > 0)
            {
                var (cost, index) = Pop();
                if (cost > dist[index]) continue;
                var cell = CellOf(index);
                foreach (var d in Neighbours)
                {
                    var next = cell + d;
                    if (!Walkable(next)) continue;
                    bool diagonal = d.x != 0 && d.y != 0;
                    if (diagonal && (!Walkable(new Vector2Int(next.x, cell.y)) || !Walkable(new Vector2Int(cell.x, next.y))))
                        continue;
                    int nc = cost + (diagonal ? Diagonal : Straight);
                    int ni = IndexOf(next);
                    if (dist[ni] >= 0 && dist[ni] <= nc) continue;
                    dist[ni] = nc;
                    Push(nc, ni);
                }
            }
        }

        // Biaya ke target (10 per sel lurus), -1 kalau terhalang, tak terjangkau, atau di luar area.
        public int Distance(Vector2Int cell) => Contains(cell) ? dist[IndexOf(cell)] : -1;

        // Tetangga berikutnya menuju target. Dari sel terhalang (badan musuh menempel penghalang) dipilih tetangga
        // terjangkau yang terdekat ke target, supaya musuh tetap punya arah keluar.
        public bool TryStep(Vector2Int from, out Vector2Int next)
        {
            next = from;
            if (!Contains(from)) return false;
            int here = dist[IndexOf(from)];
            if (here == 0) return false;
            bool fromWalkable = !blocked[IndexOf(from)];
            int best = here < 0 ? int.MaxValue : here;
            bool found = false;
            foreach (var d in Neighbours)
            {
                var c = from + d;
                int dc = Distance(c);
                if (dc < 0 || dc >= best) continue;
                bool diagonal = d.x != 0 && d.y != 0;
                if (diagonal && fromWalkable &&
                    (!Walkable(new Vector2Int(c.x, from.y)) || !Walkable(new Vector2Int(from.x, c.y))))
                    continue;
                best = dc;
                next = c;
                found = true;
            }
            return found;
        }

        bool Contains(Vector2Int c) => area.Contains(c);
        bool Walkable(Vector2Int c) => Contains(c) && !blocked[IndexOf(c)];
        int IndexOf(Vector2Int c) => (c.y - area.y) * area.width + (c.x - area.x);
        Vector2Int CellOf(int i) => new Vector2Int(area.x + i % area.width, area.y + i / area.width);

        void Push(int cost, int index)
        {
            heap.Add((cost, index));
            int i = heap.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (heap[p].cost <= heap[i].cost) break;
                (heap[p], heap[i]) = (heap[i], heap[p]);
                i = p;
            }
        }

        (int cost, int index) Pop()
        {
            var top = heap[0];
            int last = heap.Count - 1;
            heap[0] = heap[last];
            heap.RemoveAt(last);
            int i = 0;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < heap.Count && heap[l].cost < heap[m].cost) m = l;
                if (r < heap.Count && heap[r].cost < heap[m].cost) m = r;
                if (m == i) break;
                (heap[m], heap[i]) = (heap[i], heap[m]);
                i = m;
            }
            return top;
        }
    }
}
