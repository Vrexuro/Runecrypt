using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Bara Pemulih (spec Isi Makam §5.3): pulih 1 HP saat disentuh pemain, hilang setelah 10 detik.
    // Sprite unlit tanpa Light2D: terlihat di gelap tetapi tidak menerangi musuh.
    public class HealingEmber : MonoBehaviour
    {
        [SerializeField] float heal = 1f;
        [SerializeField] float lifetime = 10f;
        [SerializeField] FeedbackProfile pickupProfile;

        float born;
        bool taken;

        void Awake() => born = Time.time;

        void Update()
        {
            if (Time.time - born >= lifetime) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (taken) return;
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return; // musuh tidak mengambil bara (Review Focus 4)
            taken = true;
            player.Health.Heal(heal);
            FeedbackPlayer.Play(pickupProfile, transform.position);
            Destroy(gameObject);
        }
    }
}
