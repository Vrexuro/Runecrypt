using UnityEngine;

namespace Runecrypt.Game3D
{
    // Mengisi parameter Animator Speed dan SpeedMul dari kecepatan Rigidbody di lantai (blend tree Idle <-> Move).
    // Kecepatan fisik, bukan input: karakter yang terhalang dinding tampil Idle, bukan berlari di tempat.
    [RequireComponent(typeof(Rigidbody))]
    public class Locomotion3D : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] float designSpeed = 2f;       // v0: kecepatan yang dipakai script Blender untuk klip Move
        [SerializeField] float minMul = 0.5f;
        [SerializeField] float maxMul = 1.3f;
        [SerializeField] bool reverseWhenBackward;
        [SerializeField] Transform facing;             // arah hadap tubuh; kosong = transform ini

        const float SpeedDamp = 0.1f;
        static readonly int SpeedId = Animator.StringToHash("Speed");
        static readonly int SpeedMulId = Animator.StringToHash("SpeedMul");

        Rigidbody body;

        void Awake() => body = GetComponent<Rigidbody>();

        void Update()
        {
            if (animator == null) return;
            Vector3 v = body.linearVelocity;
            v.y = 0f;
            Vector3 fwd = (facing != null ? facing : transform).forward;
            fwd.y = 0f;
            float speed = v.magnitude;
            animator.SetFloat(SpeedId, speed, SpeedDamp, Time.deltaTime);
            animator.SetFloat(SpeedMulId, LocomotionMath.SpeedMul(speed, Vector3.Dot(v, fwd.normalized), designSpeed, minMul, maxMul, reverseWhenBackward));
        }
    }
}
