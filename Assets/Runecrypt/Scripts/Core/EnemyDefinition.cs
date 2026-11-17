using UnityEngine;

namespace Runecrypt.Core
{
    [CreateAssetMenu(menuName = "Runecrypt/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string displayName;
        public float maxHp = 12f;
        public float moveSpeed = 2f;
        public float contactDamage = 1f;
        public float contactInterval = 1f;
        public Element element = Element.None;
    }
}
