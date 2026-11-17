using UnityEngine;

namespace Runecrypt.Core
{
    [CreateAssetMenu(menuName = "Runecrypt/Reaction")]
    public class ReactionDefinition : ScriptableObject
    {
        public string displayName;
        public Element elementA;
        public Element elementB;
        public float radius;
        public float duration;
        public float damagePerTick;
        public float tickInterval;
        public float blindDuration;
        public GameObject effectPrefab;
        public FeedbackProfile spawnProfile;

        public bool Matches(Element x, Element y) =>
            (x == elementA && y == elementB) || (x == elementB && y == elementA);
    }
}
