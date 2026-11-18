using System;
using System.Collections.Generic;
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
        [SerializeField] LightSourceBehaviour lightSource;
        [SerializeField] float baseLightRadius = 3f;

        [Header("Kobaran (GDD §4.1)")]
        [SerializeField] BlazeRing blazePrefab;
        [SerializeField] float blazeCooldown = 6f;
        [SerializeField] float blazeRadius = 2.5f;
        [SerializeField] float blazeDamage = 6f;

        [Header("Umpan balik (M1.5)")]
        [SerializeField] FeedbackProfile shotProfile;
        [SerializeField] FeedbackProfile hurtProfile;
        [SerializeField] HitFlash hitFlash;
        [SerializeField] LightPulse lightPulse;
        [SerializeField] SpriteRenderer bodySprite;

        const float SpreadDegrees = 10f, ShotPulse = 1.3f, ShotPulseTime = 0.05f, BlinkInterval = 0.08f, BlinkAlpha = 0.4f;

        public static PlayerController Instance { get; private set; }
        public Health Health { get; private set; }
        public event Action Died;
        public PlayerStats Stats { get; } = new PlayerStats();
        public IReadOnlyList<RelicDefinition> Relics => relics;

        readonly List<RelicDefinition> relics = new List<RelicDefinition>();
        float speed;

        RunecryptControls controls;
        Rigidbody2D body;
        Camera cam;
        Cooldown fire, dash, blaze;
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
            blaze = new Cooldown(blazeCooldown);
            Health = new Health(maxHp);
            Health.Died += OnDied;
            Recalculate();
        }

        // Relic bertumpuk lewat stat (GDD §5.2). Kode pemain tidak berubah untuk relic baru.
        public void AddRelic(RelicDefinition r)
        {
            if (r == null || Health.IsDead) return;
            relics.Add(r);
            Stats.Add(r.mods);
            Recalculate();
            if (r.healFull) Health.Heal(Health.Max);
            else if (r.heal > 0f) Health.Heal(r.heal);
        }

        void Recalculate()
        {
            fire.Seconds = 1f / Mathf.Max(0.5f, Stats.Get(Stat.FireRate, shotsPerSecond));
            dash.Seconds = Stats.Get(Stat.DashCooldown, dashCooldown);
            blaze.Seconds = Stats.Get(Stat.BlazeCooldown, blazeCooldown);
            speed = Stats.Get(Stat.MoveSpeed, moveSpeed);
            Health.SetMax(Stats.Get(Stat.MaxHp, maxHp));
            if (lightSource != null) lightSource.Radius = Stats.Get(Stat.LightRadius, baseLightRadius);
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
                Shoot(aim);
            }

            if (bodySprite != null)
            {
                bool dim = now < blinkUntil && (int)((blinkUntil - now) / BlinkInterval) % 2 == 0;
                SetBodyAlpha(dim ? BlinkAlpha : 1f);
            }

            if (g.Blaze.WasPressedThisFrame() && blaze.IsReady(now) && blazePrefab != null)
            {
                blaze.Start(now);
                var shot = Stats.Shot(fireballPrefab.Defaults);
                Instantiate(blazePrefab, body.position, Quaternion.identity)
                    .Init(Stats.Get(Stat.BlazeDamage, blazeDamage), blazeRadius, shot.BurnDuration, shot.BurnDamagePerTick);
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

        // Satu tembakan: Bara Kembar menambah bola api yang menyebar 10° (GDD §5.2).
        internal void Shoot(Vector2 direction)
        {
            var shot = Stats.Shot(fireballPrefab.Defaults);
            Vector2 muzzle = body.position + direction * muzzleOffset;
            foreach (var dir in SpreadPattern.Directions(direction, shot.Count, SpreadDegrees))
                Instantiate(fireballPrefab, muzzle, Quaternion.identity).Launch(dir, shot);
            FeedbackPlayer.Play(shotProfile, muzzle);
            if (lightPulse != null) lightPulse.Pulse(ShotPulse, ShotPulseTime);
            FeedbackPlayer.Kick(-direction, 1); // sentakan 1 piksel, bukan guncangan (anti-tujuan spec §2)
        }

        void FixedUpdate()
        {
            body.linearVelocity = Time.time < dashUntil
                ? dashVelocity
                : Vector2.ClampMagnitude(controls.Gameplay.Move.ReadValue<Vector2>(), 1f) * speed;
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
