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
        [Tooltip("Detik muatan elemen kosong setelah memicu reaksi (GDD §5.1).")]
        public float chargeCooldown = 4f;
        [Tooltip("Benda yang ditinggalkan saat mati, misalnya Genangan Beku (Biarawan Beku).")]
        public GameObject dropOnDeath;
        [Tooltip("Biaya dalam budget gelombang (rencana M3a Task 5).")]
        public int cost = 1;

        [Header("Umpan balik (M1.5). Gaya mati per jenis musuh cukup diganti di sini.")]
        public FeedbackProfile hitLit;
        public FeedbackProfile hitDark;
        public FeedbackProfile deathProfile;
        public GameObject deathEffect;
        public GameObject remains;
    }
}
