using UnityEngine;

namespace Runecrypt.Game3D
{
    // Kamera miring yang mengikuti pemain dengan halus. Jarak dan sudut diambil dari posisi awal di scene.
    public class CameraFollow3D : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float smoothing = 8f;

        Vector3 offset;

        void Start()
        {
            if (target == null && Player3D.Instance != null) target = Player3D.Instance.transform;
            if (target != null) offset = transform.position - target.position;
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, target.position + offset, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }
    }
}
