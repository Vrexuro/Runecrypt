using Runecrypt.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Runecrypt.Game
{
    // Radius aturan cahaya dan radius Light2D selalu satu angka (GDD §5.5).
    public class LightSourceBehaviour : MonoBehaviour, ILightSource
    {
        [SerializeField] float radius = 3f;
        [SerializeField] Light2D light2D;

        public float Radius
        {
            get => radius;
            set { radius = value; Sync(); }
        }

        public Vector2 Position => transform.position;
        public bool IsOn => isActiveAndEnabled;

        void Awake() => Sync();
        void OnValidate() => Sync();
        void OnEnable() => LightFieldHost.Field.Register(this);
        void OnDisable() => LightFieldHost.Field.Unregister(this);

        void Sync()
        {
            if (light2D == null) light2D = GetComponent<Light2D>();
            if (light2D != null) light2D.pointLightOuterRadius = radius;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.4f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
