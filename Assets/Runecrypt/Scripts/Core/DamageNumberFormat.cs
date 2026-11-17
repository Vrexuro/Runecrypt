using System;
using UnityEngine;

namespace Runecrypt.Core
{
    public enum HitKind { Lit, Dark, Burn, Steam }

    // Angka damage (spec §5.3): dibulatkan ke bilangan bulat terdekat, minimal 1. Terang = kuning besar.
    public static class DamageNumberFormat
    {
        public static int Value(float damage) =>
            Math.Max(1, (int)Math.Round(damage, MidpointRounding.AwayFromZero));

        public static (Color color, float scale) Style(HitKind kind) => kind switch
        {
            HitKind.Lit => (Rgb(0xFF, 0xE1, 0x4D), 2f),
            HitKind.Dark => (Rgb(0x8A, 0x8A, 0x99), 1f),
            HitKind.Burn => (Rgb(0xFF, 0x8A, 0x3D), 1f),
            _ => (Rgb(0xE8, 0xE4, 0xD8), 1f),
        };

        static Color Rgb(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);
    }
}
