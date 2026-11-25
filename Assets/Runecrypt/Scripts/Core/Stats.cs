using System;
using System.Collections.Generic;

namespace Runecrypt.Core
{
    // Stat yang bisa diubah relic dan upgrade (GDD §4.2, §5.2, §5.3). Stat baru = nilai enum baru + pembacanya.
    public enum Stat
    {
        Damage, FireRate, Range, ProjectileSpeed, ProjectileSize, ProjectileCount, Pierce, Bounce, HomingTurn,
        BurnDuration, BurnDamage, LightRadius, ProjectileLight, MaxHp, MoveSpeed, DashCooldown, BlazeCooldown, BlazeDamage,
        SteamRadius, SteamDuration, // M3a, Mahkota Asap. Hanya boleh menambah di ujung: aset relic menyimpan angka enum.
    }

    public enum StatOp { Add, Percent }

    // Percent berupa pecahan: 0.2 = +20%.
    [Serializable]
    public struct StatMod
    {
        public Stat stat;
        public StatOp op;
        public float value;
    }

    // Hasil stat untuk satu tembakan bola api.
    public struct ShotStats
    {
        public float Damage, Speed, Range, Size, LightScale, HomingTurn, BurnDuration, BurnDamagePerTick;
        public int Count, Pierce, Bounce;
    }

    // Relic bertumpuk: nilai akhir = (dasar + ΣAdd) × (1 + ΣPercent), tidak pernah negatif.
    public class PlayerStats
    {
        static readonly int Count = Enum.GetValues(typeof(Stat)).Length;
        readonly float[] add = new float[Count];
        readonly float[] percent = new float[Count];

        public void Add(IEnumerable<StatMod> mods)
        {
            if (mods == null) return;
            foreach (var m in mods)
            {
                if (m.op == StatOp.Add) add[(int)m.stat] += m.value;
                else percent[(int)m.stat] += m.value;
            }
        }

        public float Get(Stat s, float baseValue) =>
            Math.Max(0f, (baseValue + add[(int)s]) * (1f + percent[(int)s]));

        public int GetInt(Stat s, int baseValue) => (int)Math.Floor(Get(s, baseValue) + 1e-4f);

        public ShotStats Shot(ShotStats b) => new ShotStats
        {
            Damage = Get(Stat.Damage, b.Damage),
            Speed = Math.Max(1f, Get(Stat.ProjectileSpeed, b.Speed)),
            Range = Math.Max(1f, Get(Stat.Range, b.Range)),
            Size = Math.Max(0.25f, Get(Stat.ProjectileSize, b.Size)),
            LightScale = Get(Stat.ProjectileLight, b.LightScale),
            HomingTurn = Get(Stat.HomingTurn, b.HomingTurn),
            BurnDuration = Get(Stat.BurnDuration, b.BurnDuration),
            BurnDamagePerTick = Get(Stat.BurnDamage, b.BurnDamagePerTick),
            Count = Math.Max(1, GetInt(Stat.ProjectileCount, b.Count)),
            Pierce = GetInt(Stat.Pierce, b.Pierce),
            Bounce = GetInt(Stat.Bounce, b.Bounce),
        };
    }
}
