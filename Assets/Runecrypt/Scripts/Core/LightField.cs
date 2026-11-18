using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runecrypt.Core
{
    public interface ILightSource
    {
        Vector2 Position { get; }
        float Radius { get; }
        bool IsOn { get; }
    }

    // Aturan cahaya GDD §5.5 dihitung di sini, bukan dibaca dari Light2D.
    public class LightField
    {
        readonly Func<Vector2, Vector2, bool> isBlocked;
        // shortcut: linear scan, upgrade to a spatial grid if > 64 lights
        readonly List<ILightSource> sources = new List<ILightSource>();

        public LightField(Func<Vector2, Vector2, bool> isBlocked) => this.isBlocked = isBlocked;

        public void Register(ILightSource s)
        {
            if (!sources.Contains(s)) sources.Add(s);
        }

        public void Unregister(ILightSource s) => sources.Remove(s);

        public bool IsLit(Vector2 point) => IsLit(point, null);

        // ignore: cahaya milik bola api yang sedang mencari sasaran tidak ikut dihitung (GDD §5.5 aturan 2).
        public bool IsLit(Vector2 point, ILightSource ignore)
        {
            foreach (var s in sources)
            {
                if (!s.IsOn || s == ignore) continue;
                if ((point - s.Position).sqrMagnitude > s.Radius * s.Radius) continue;
                if (!isBlocked(s.Position, point)) return true;
            }
            return false;
        }
    }
}
