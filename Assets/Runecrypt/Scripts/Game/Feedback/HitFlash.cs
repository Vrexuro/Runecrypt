using UnityEngine;

namespace Runecrypt.Game
{
    // Overlay kedipan: child SpriteRenderer bermaterial Runecrypt/SpriteFlash yang menyalin sprite induk.
    // Warnanya ditulis ulang setiap LateUpdate, jadi fade spawn ArenaDirector tidak bisa membuatnya putih.
    [RequireComponent(typeof(SpriteRenderer))]
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] SpriteRenderer source;

        SpriteRenderer sr;
        Color color = Color.white;
        float alpha, until;

        SpriteRenderer Sr => sr != null ? sr : sr = GetComponent<SpriteRenderer>();

        public void SetSource(SpriteRenderer s) => source = s;

        public void Flash(Color c, float a, float duration)
        {
            color = c;
            alpha = a;
            until = Time.time + duration;
        }

        public void Apply(float now)
        {
            if (source != null)
            {
                Sr.sprite = source.sprite;
                Sr.flipX = source.flipX;
                Sr.sortingLayerID = source.sortingLayerID;
                Sr.sortingOrder = source.sortingOrder + 1;
            }
            Sr.color = new Color(color.r, color.g, color.b, now < until ? alpha : 0f);
        }

        // LateUpdate: setelah SpriteFlipbook mengganti frame induk di Update.
        void LateUpdate() => Apply(Time.time);
    }
}
