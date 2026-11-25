using System;
using System.Collections.Generic;

namespace Runecrypt.Core
{
    // Gelombang campuran (rencana M3a Task 5): budget = WaveSchedule.NextWaveSize(), biaya per jenis musuh
    // dari EnemyDefinition.cost. Rumus per ruangan dan per lantai yang sebenarnya dikerjakan di M4 (GDD §9.3).
    public static class SpawnBudget
    {
        // Memilih indeks dari costs secara acak sampai budget habis, tidak ada yang terjangkau lagi, atau mencapai cap.
        public static List<int> Compose(int budget, IReadOnlyList<int> costs, Random rng, int cap)
        {
            var picks = new List<int>();
            var affordable = new List<int>();
            int left = budget;
            while (picks.Count < cap)
            {
                affordable.Clear();
                for (int i = 0; i < costs.Count; i++)
                    if (Cost(costs[i]) <= left) affordable.Add(i);
                if (affordable.Count == 0) break;
                int pick = affordable[rng.Next(affordable.Count)];
                picks.Add(pick);
                left -= Cost(costs[pick]);
            }
            return picks;
        }

        static int Cost(int c) => Math.Max(1, c); // biaya 0 dari Inspector dihitung 1, supaya pasti berhenti
    }
}
