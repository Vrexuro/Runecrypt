using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game3D
{
    // Sumber cahaya untuk aturan cahaya GDD §5.5 di 3D. Radius dalam tile, dihitung di bidang lantai.
    // Kalau ada Light (point), jangkauannya diselaraskan supaya yang terlihat terang memang dihitung terang.
    public class LightSource3D : MonoBehaviour, ILightSource
    {
        [SerializeField] float radius = 3f;
        [SerializeField] Light visual;
        [SerializeField] float rangePerRadius = 1.4f;

        public float Radius
        {
            get => radius;
            set { radius = value; SyncVisual(); }
        }
        public Vector2 Position => Plane3D.ToPlane(transform.position);
        public bool IsOn => isActiveAndEnabled;

        void OnEnable()
        {
            LightFieldHost3D.Field.Register(this);
            SyncVisual();
        }

        void OnDisable() => LightFieldHost3D.Field.Unregister(this);

        void SyncVisual()
        {
            if (visual != null) visual.range = radius * rangePerRadius;
        }
    }
}
