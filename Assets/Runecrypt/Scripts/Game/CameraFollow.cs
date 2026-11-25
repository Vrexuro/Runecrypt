using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Rig kamera (spec M2.6 §5.1): mengikuti pemain dengan halus, dibatasi tepi ruangan, dibulatkan ke piksel.
    // Kamera adalah anak rig, jadi CameraShake cukup menggeser localPosition kamera.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Rect room;
        [SerializeField] Vector2 halfView = new Vector2(10f, 5.625f); // 320x180 px pada 16 PPU (Pixel Perfect Camera)
        [SerializeField] float smoothTime = 0.08f;

        const int Ppu = 16;
        Vector2 pos, velocity;

        void Start()
        {
            if (target == null && PlayerController.Instance != null) target = PlayerController.Instance.transform;
            pos = target != null ? CameraBounds.Clamp(target.position, room, halfView) : (Vector2)transform.position;
            Apply();
        }

        void LateUpdate()
        {
            if (target == null)
            {
                if (PlayerController.Instance == null) return;
                target = PlayerController.Instance.transform;
            }
            Vector2 goal = CameraBounds.Clamp(target.position, room, halfView);
            pos = smoothTime > 0f ? Vector2.SmoothDamp(pos, goal, ref velocity, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime) : goal;
            Apply();
        }

        void Apply()
        {
            Vector2 p = CameraBounds.SnapToPixel(pos, Ppu);
            transform.position = new Vector3(p.x, p.y, transform.position.z);
        }
    }
}
