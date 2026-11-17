using System;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Pyromancer (GDD §4.1): gerak, bidik 360°, Bola Api, dash dengan i-frame.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHp = 6f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float shotsPerSecond = 3f;
        [SerializeField] float dashDistance = 3f;
        [SerializeField] float dashCooldown = 0.8f;
        [SerializeField] float dashDuration = 0.15f;
        [SerializeField] float dashInvulnerability = 0.2f;
        [SerializeField] float hitInvulnerability = 0.8f;
        [SerializeField] float muzzleOffset = 0.5f;
        [SerializeField] Fireball fireballPrefab;

        [Header("Umpan balik (M1.5)")]
        [SerializeField] FeedbackProfile shotProfile;
        [SerializeField] FeedbackProfile hurtProfile;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] LightPulse lightPulse;
        [SerializeField] SpriteRenderer bodySprite;

        const float ShotPulse = 1.3f, ShotPulseTime = 0.05f, BlinkInterval = 0.08f, BlinkAlpha = 0.4f;

        public static PlayerController Instance { get; private set; }
        public Health Health { get; private set; }
        public event Action Died;

        RunecryptControls controls;
        Rigidbody2D body;
        Camera cam;
        Cooldown fire, dash;
        Vector2 aim = Vector2.right;
        Vector2 dashVelocity;
        float dashUntil, invulnerableUntil, blinkUntil;

        void Awake()
        {
            Instance = this;
            body = GetComponent<Rigidbody2D>();
            cam = Camera.main;
            controls = new RunecryptControls();
            fire = new Cooldown(1f / shotsPerSecond);
            dash = new Cooldown(dashCooldown);
            Health = new Health(maxHp);
            Health.Died += OnDied;
        }

        void OnEnable() => controls.Enable();
        void OnDisable() => controls.Disable();

        void OnDestroy()
        {
            controls.Dispose();
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            var g = controls.Gameplay;
            float now = Time.time;

            Vector2 mouseWorld = cam.ScreenToWorldPoint(g.AimPoint.ReadValue<Vector2>());
            var aimResult = AimSelector.Select(g.AimStick.ReadValue<Vector2>(), g.AimKeys.ReadValue<Vector2>(), mouseWorld, body.position, aim);
            aim = aimResult.Direction;

            // WasPressedThisFrame: klik singkat yang turun-naik di antara dua frame tetap menembak.
            bool wantFire = aimResult.AutoFire || g.Fire.IsPressed() || g.Fire.WasPressedThisFrame();
            if (wantFire && fire.IsReady(now))
            {
                fire.Start(now);
                Vector2 muzzle = body.position + aim * muzzleOffset;
                var fb = Instantiate(fireballPrefab, muzzle, Quaternion.identity);
                fb.Launch(aim);
                FeedbackPlayer.Play(shotProfile, muzzle);
                if (lightPulse != null) lightPulse.Pulse(ShotPulse, ShotPulseTime);
                FeedbackPlayer.Kick(-aim, 1); // sentakan 1 piksel, bukan guncangan (anti-tujuan spec §2)
            }

            if (bodySprite != null)
            {
                bool dim = now < blinkUntil && (int)((blinkUntil - now) / BlinkInterval) % 2 == 0;
                SetBodyAlpha(dim ? BlinkAlpha : 1f);
            }

            if (g.Dash.WasPressedThisFrame() && dash.IsReady(now))
            {
                var move = g.Move.ReadValue<Vector2>();
                var dir = move.sqrMagnitude > 0.01f ? move.normalized : aim;
                dashVelocity = dir * (dashDistance / dashDuration);
                dashUntil = now + dashDuration;
                invulnerableUntil = now + dashInvulnerability;
                dash.Start(now);
            }
        }

        void FixedUpdate()
        {
            body.linearVelocity = Time.time < dashUntil
                ? dashVelocity
                : Vector2.ClampMagnitude(controls.Gameplay.Move.ReadValue<Vector2>(), 1f) * moveSpeed;
        }

        public void TakeHit(HitInfo hit)
        {
            if (Health.IsDead || Time.time < invulnerableUntil) return;
            Health.TakeDamage(hit.Damage);
            // Kebal sesaat setelah kena, supaya dikerubungi tidak berarti mati dalam 2 detik.
            invulnerableUntil = Time.time + hitInvulnerability;
            blinkUntil = invulnerableUntil;
            FeedbackPlayer.Play(hurtProfile, body.position, hitFlash);
        }

        void SetBodyAlpha(float a)
        {
            var c = bodySprite.color;
            if (c.a != a) bodySprite.color = new Color(c.r, c.g, c.b, a);
        }

        void OnDied()
        {
            if (bodySprite != null) SetBodyAlpha(1f); // Update berhenti setelah ini; jangan tertinggal transparan
            body.linearVelocity = Vector2.zero;
            enabled = false;
            Died?.Invoke();
        }
    }
}
