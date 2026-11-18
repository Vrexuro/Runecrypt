using UnityEngine;

namespace Runecrypt.Game
{
    // Garis tepi jingga untuk musuh Tersingkap (GDD §5.5 aturan 1): child SpriteRenderer bermaterial
    // Runecrypt/SpriteOutline yang menyalin sprite induk, satu urutan di belakangnya. Polanya sama dengan HitFlash.
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExposedOutline : MonoBehaviour
    {
        [SerializeField] SpriteRenderer source;

        SpriteRenderer sr;
        bool visible;

        SpriteRenderer Sr => sr != null ? sr : sr = GetComponent<SpriteRenderer>();

        public void SetSource(SpriteRenderer s) => source = s;
        public void SetVisible(bool v) => visible = v;

        public void Apply()
        {
            if (source != null)
            {
                Sr.sprite = source.sprite;
                Sr.flipX = source.flipX;
                Sr.sortingLayerID = source.sortingLayerID;
                Sr.sortingOrder = source.sortingOrder - 1;
            }
            Sr.enabled = visible && source != null && source.enabled;
        }

        // LateUpdate: setelah SpriteFlipbook mengganti frame induk di Update.
        void LateUpdate() => Apply();
    }
}
