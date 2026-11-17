using System.Collections.Generic;

namespace Runecrypt.Core
{
    public static class ReactionResolver
    {
        public static ReactionDefinition Find(Element x, Element y, IReadOnlyList<ReactionDefinition> book)
        {
            if (x == y || x == Element.None || y == Element.None) return null;
            foreach (var def in book)
                if (def != null && def.Matches(x, y)) return def;
            return null;
        }
    }
}
