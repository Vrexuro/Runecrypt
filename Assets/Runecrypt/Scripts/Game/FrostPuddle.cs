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

    // Genangan Beku (GDD §5.1): memicu reaksi saat kena elemen lain, hilang, lalu muncul lagi.
    public class FrostPuddle : MonoBehaviour, IDamageable
    {
        [SerializeField] Element element = Element.Ice;
        [SerializeField] List<ReactionDefinition> reactionBook = new List<ReactionDefinition>();
        [SerializeField] float respawnSeconds = 8f;

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
            var fx = Instantiate(def.effectPrefab, transform.position, Quaternion.identity);
            fx.GetComponent<IReactionEffect>()?.Init(def);
            sr.enabled = false;
            col.enabled = false;
        }
    }
}
