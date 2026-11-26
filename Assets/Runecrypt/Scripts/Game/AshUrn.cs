using System;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Guci Abu (spec Isi Makam §5.3): pecah oleh pukulan, kadang menjatuhkan Bara Pemulih, utuh lagi saat ruangan
    // dimulai ulang. Musuh tidak memukul guci, jadi hadiahnya milik pemain.
    public class AshUrn : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHp = 3f;
        [SerializeField, Range(0f, 1f)] float dropChance = 0.15f;
        [SerializeField] HealingEmber emberPrefab;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Sprite wholeSprite;
        [SerializeField] Sprite brokenSprite;
        [SerializeField] BoxCollider2D solid;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] FeedbackProfile breakProfile;

        const int WholeOrder = 0, BrokenOrder = -8; // pecahan di bawah karakter, di atas lantai
        const float FlashTime = 0.08f;

        // Undian ke-n run ini dari stream "loot" (mengikuti seed). Bisa diganti test.
        internal Func<float> random = () => (float)Run.Random.Stream("loot").NextDouble();

        BreakableState state;
        bool restorePending;
        int playerMask;

        public bool IsBroken => state.IsBroken;

        void Awake()
        {
            state = new BreakableState(maxHp);
            playerMask = LayerMask.GetMask("Player");
        }

        void OnEnable() => RoomEvents.Started += OnRoomStarted;
        void OnDisable() => RoomEvents.Started -= OnRoomStarted;

        void Update()
        {
            if (restorePending) TryRestore();
        }

        public void TakeHit(HitInfo hit)
        {
            if (state.IsBroken) return;
            if (!state.Hit(hit.Damage))
            {
                if (hitFlash != null) hitFlash.Flash(Color.white, 1f, FlashTime);
                return;
            }
            Vector2 pos = transform.position;
            body.sprite = brokenSprite;
            body.sortingOrder = BrokenOrder;
            solid.enabled = false;
            FeedbackPlayer.Play(breakProfile, pos);
            if (emberPrefab != null && Loot.Roll(dropChance, random())) Instantiate(emberPrefab, pos, Quaternion.identity);
        }

        void OnRoomStarted()
        {
            if (!state.IsBroken) return;
            restorePending = true;
            TryRestore();
        }

        // Guci tidak muncul di atas pemain (Review Focus 1); ditunda sampai area kosong.
        void TryRestore()
        {
            // Collider sedang nonaktif, jadi bounds-nya kosong; area dihitung dari offset dan ukuran kotak.
            Vector2 center = transform.TransformPoint(solid.offset);
            Vector2 size = Vector2.Scale(solid.size, transform.lossyScale);
            if (Physics2D.OverlapBox(center, size, 0f, playerMask) != null) return;
            restorePending = false;
            state.Reset();
            body.sprite = wholeSprite;
            body.sortingOrder = WholeOrder;
            solid.enabled = true;
        }
    }
}
