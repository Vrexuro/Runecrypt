using UnityEngine;

namespace Runecrypt.Core
{
    // Umpan balik satu kejadian (spec §6). Di Core supaya EnemyDefinition dan ReactionDefinition bisa menunjuknya.
    [CreateAssetMenu(menuName = "Runecrypt/Feedback")]
    public class FeedbackProfile : ScriptableObject
    {
        [Header("Kedipan")]
        public Color flashColor = Color.white;
        public float flashAlpha;
        public float flashDuration;

        [Header("Waktu dan kamera")]
        public float hitStop;
        public float shakeTrauma;

        [Header("Partikel dan suara")]
        public GameObject particlePrefab;
        public AudioClip[] clips;
        public float volume = 1f;
        public float pitchJitter = 0.05f;
    }
}
