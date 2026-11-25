using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Satu tempat untuk memunculkan efek reaksi (Uap, nanti Ledakan Cahaya) dari benda maupun musuh.
    public static class ReactionSpawner
    {
        public static GameObject Spawn(ReactionDefinition def, Vector2 pos)
        {
            if (def == null || def.effectPrefab == null) return null;
            var fx = Object.Instantiate(def.effectPrefab, pos, Quaternion.identity);
            fx.GetComponent<IReactionEffect>()?.Init(def);
            return fx;
        }
    }
}
