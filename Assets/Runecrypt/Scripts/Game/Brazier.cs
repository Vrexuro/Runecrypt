using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Anglo (spec Isi Makam §5.2): menyala oleh serangan api dan menjadi sumber cahaya aturan (radius 2,5),
    // padam lagi saat ruangan dimulai ulang. Cahaya dan apinya ada di anak "Fire", aktif hanya saat menyala,
    // jadi LightSourceBehaviour di sana otomatis masuk dan keluar dari LightField.
    public class Brazier : MonoBehaviour, IDamageable
    {
        [SerializeField] SpriteRenderer unlitBody;   // sprite anglo padam
        [SerializeField] GameObject fire;            // flipbook anglo menyala + Light2D + LightSourceBehaviour
        [SerializeField] LightPulse pulse;
        [SerializeField] FeedbackProfile igniteProfile;

        const float PulseMult = 2f, PulseTime = 0.25f;

        readonly BrazierState state = new BrazierState();

        public bool IsLit => state.IsLit;

        void Awake() => Apply();
        void OnEnable() => RoomEvents.Started += OnRoomStarted;
        void OnDisable() => RoomEvents.Started -= OnRoomStarted;

        public void TakeHit(HitInfo hit)
        {
            if (!state.TryIgnite(hit.Element)) return;
            Apply();
            if (pulse != null) pulse.Pulse(PulseMult, PulseTime);
            FeedbackPlayer.Play(igniteProfile, transform.position);
        }

        void OnRoomStarted()
        {
            state.Reset();
            Apply();
        }

        void Apply()
        {
            if (fire != null) fire.SetActive(state.IsLit);
            if (unlitBody != null) unlitBody.enabled = !state.IsLit;
        }
    }
}
