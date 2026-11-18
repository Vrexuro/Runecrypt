using System.Collections.Generic;

namespace Runecrypt.Core
{
    // Dipakai pickup arena (M2), ruang harta (M4), dan peti boss (M5).
    public static class RelicPicker
    {
        public static RelicDefinition PickUnowned(IReadOnlyList<RelicDefinition> pool,
            IReadOnlyCollection<RelicDefinition> owned, System.Random rng)
        {
            var candidates = new List<RelicDefinition>();
            foreach (var r in pool)
                if (r != null && !Contains(owned, r)) candidates.Add(r);
            return candidates.Count == 0 ? null : candidates[rng.Next(candidates.Count)];
        }

        static bool Contains(IReadOnlyCollection<RelicDefinition> owned, RelicDefinition r)
        {
            foreach (var o in owned) if (o == r) return true;
            return false;
        }
    }
}
