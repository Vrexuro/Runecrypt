using System;
using Runecrypt.Core;
using Runecrypt.Game;
using UnityEngine;

namespace Runecrypt.Game3D
{
    // Pyromancer 3D (GDD §4.1): WASD, bidik mouse ke lantai (atau stick/panah lewat AimSelector), tembak Bola Api.
    // Logika HP, cooldown, dan bidikan dari Runecrypt.Core; hanya gerak dan bidik ke lantai yang khas 3D.
    [RequireComponent(typeof(Rigidbody))]
    public class Player3D : MonoBehaviour, IDamageable
    {
        [SerializeField] float maxHp = 6f;
        [SerializeField] float moveSpeed = 5f;
        [SerializeField] float shotsPerSecond = 3f;
        [SerializeField] float hitInvulnerability = 0.8f;
        [SerializeField] float muzzleOffset = 0.5f;
        [SerializeField] float muzzleHeight = 0.5f;
        [SerializeField] Fireball3D fireballPrefab;
        [SerializeField] Transform visual;             // tubuh yang diputar menghadap arah bidik

        public static Player3D Instance { get; private set; }
        public Health Health { get; private set; }
        public event Action Died;
        public Vector2 Aim => aim;
        // bola api terbang setinggi moncong; bidik di lantai membuat tembakan ke samping lewat di belakang musuh
        internal float AimPlaneHeight => transform.position.y + muzzleHeight;

        RunecryptControls controls;
        Rigidbody body;
        Camera cam;
        Cooldown fire;
        Vector2 aim = Vector2.up;
        float invulnerableUntil;

        void Awake()
        {
            Instance = this;
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            // Review Focus 4: tidak terlempar dari lantai dan tidak terguling.
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            Health = new Health(maxHp);
            Health.Died += OnDied;
            fire = new Cooldown(1f / shotsPerSecond);
            controls = new RunecryptControls();
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
            if (Health.IsDead) return;
            var g = controls.Gameplay;
            if (cam == null) cam = Camera.main;
            Vector2 pos = Plane3D.ToPlane(transform.position);
            // Review Focus 1: kursor yang tidak mengenai lantai memakai arah bidik terakhir.
            Vector2 mouse = pos + aim;
            if (cam != null && Plane3D.RayToFloor(cam.ScreenPointToRay(g.AimPoint.ReadValue<Vector2>()), AimPlaneHeight, out var p)) mouse = p;
            var result = AimSelector.Select(g.AimStick.ReadValue<Vector2>(), g.AimKeys.ReadValue<Vector2>(), mouse, pos, aim);
            aim = result.Direction;
            if (visual != null) visual.rotation = Quaternion.LookRotation(Plane3D.ToWorld(aim), Vector3.up);

            bool wantFire = result.AutoFire || g.Fire.IsPressed() || g.Fire.WasPressedThisFrame();
            float now = Time.time;
            if (wantFire && fire.IsReady(now))
            {
                fire.Start(now);
                Shoot(aim);
            }
        }

        void FixedUpdate()
        {
            Vector2 move = Health.IsDead ? Vector2.zero : Vector2.ClampMagnitude(controls.Gameplay.Move.ReadValue<Vector2>(), 1f) * moveSpeed;
            body.linearVelocity = Plane3D.ToWorld(move);
        }

        internal void Shoot(Vector2 dir)
        {
            if (fireballPrefab == null) return;
            Vector3 at = transform.position + Plane3D.ToWorld(dir * muzzleOffset, muzzleHeight);
            var f = Instantiate(fireballPrefab, at, Quaternion.LookRotation(Plane3D.ToWorld(dir), Vector3.up));
            f.Launch(dir);
        }

        public void TakeHit(HitInfo hit)
        {
            if (Health.IsDead || Time.time < invulnerableUntil) return;
            Health.TakeDamage(hit.Damage);
            invulnerableUntil = Time.time + hitInvulnerability;
        }

        void OnDied()
        {
            body.linearVelocity = Vector3.zero;
            Died?.Invoke();
        }
    }
}
