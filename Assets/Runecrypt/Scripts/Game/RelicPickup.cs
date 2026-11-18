using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Relic yang tergeletak di lantai. Disentuh pemain sekali, relic masuk, pickup hilang (GDD §4.3).
    public class RelicPickup : MonoBehaviour
    {
        [SerializeField] RelicDefinition relic;
        [SerializeField] FeedbackProfile pickupProfile;

        bool taken;

        public RelicDefinition Relic => relic;
        public void Init(RelicDefinition r) => relic = r;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken || relic == null) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;
            taken = true;
            player.AddRelic(relic);
            FeedbackPlayer.Play(pickupProfile, transform.position);
            Destroy(gameObject);
        }
    }
}
