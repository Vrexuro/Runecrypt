using System.Collections.Generic;
using Runecrypt.Core;
using UnityEngine;

namespace Runecrypt.Game
{
    // Efek reaksi (Uap, nanti Ledakan Cahaya) dibuat dari prefab di ReactionDefinition.
    public interface IReactionEffect
    {
        void Init(ReactionDefinition def);
    }

    // Benda reaksi (GDD §5.1). Genangan Beku: memicu reaksi saat kena elemen lain, hilang, lalu muncul lagi.
    // Kristal Es (M3a): singleUse, pecah setelah bereaksi dan pecahannya melukai musuh di dekatnya, tidak pemain.
    public class ReactiveProp : MonoBehaviour, IDamageable
    {
        [SerializeField] Element element = Element.Ice;
        [SerializeField] List<ReactionDefinition> reactionBook = new List<ReactionDefinition>();
        [SerializeField] float respawnSeconds = 8f;
        [SerializeField] bool singleUse;
        [SerializeField] float shatterDamage;
        [SerializeField] float shatterRadius = 1.5f;

        Cooldown cooldown;
        SpriteRenderer sr;
        Collider2D col;

        void Awake()
        {
            cooldown = new Cooldown(respawnSeconds);
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
        }

        void Update()
        {
            bool ready = cooldown.IsReady(Time.time);
            if (sr.enabled == ready) return;
            sr.enabled = ready;
            col.enabled = ready;
        }

        public void TakeHit(HitInfo hit)
        {
            float now = Time.time;
            if (!cooldown.IsReady(now)) return;
            var def = ReactionResolver.Find(hit.Element, element, reactionBook);
            if (def == null || def.effectPrefab == null) return;
            cooldown.Start(now); // sebelum spawn: tembakan lain di frame yang sama tidak memicu dua kali
            ReactionSpawner.Spawn(def, transform.position);
            sr.enabled = false;
            col.enabled = false;
            if (!singleUse) return;
            Shatter();
            Destroy(gameObject);
        }

        void Shatter()
        {
            if (shatterDamage <= 0f) return;
            Vector2 pos = transform.position;
            var struck = new HashSet<Enemy>();
            foreach (var c in Physics2D.OverlapCircleAll(pos, shatterRadius))
            {
                var enemy = c.GetComponentInParent<Enemy>(); // hanya musuh; pemain tidak terluka (GDD §5.1)
                if (enemy != null && struck.Add(enemy)) enemy.TakeHit(new HitInfo(shatterDamage, element, false, pos));
            }
        }
    }
}
