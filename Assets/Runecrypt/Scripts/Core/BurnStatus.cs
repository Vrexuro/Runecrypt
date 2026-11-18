namespace Runecrypt.Core
{
    // Status Terbakar (GDD §4.1): damage per tick sampai durasi habis.
    // Apply ulang hanya memperpanjang durasi, jadwal tick tidak di-reset.
    public class BurnStatus
    {
        const float Epsilon = 1e-4f;
        readonly float tickInterval;
        float damagePerTick, duration;
        float end = float.NegativeInfinity;
        float nextTick;

        public BurnStatus(float damagePerTick = 1f, float duration = 2f, float tickInterval = 1f)
        {
            this.damagePerTick = damagePerTick;
            this.duration = duration;
            this.tickInterval = tickInterval;
        }

        public bool IsActive(float now) => now <= end + Epsilon;

        public void Apply(float now) => Apply(now, duration, damagePerTick);

        // Durasi dan damage mengikuti stat penyerang (Abu Pekat, Bara Tak Padam); apply terakhir yang berlaku.
        public void Apply(float now, float duration, float damagePerTick)
        {
            this.duration = duration;
            this.damagePerTick = damagePerTick;
            if (!IsActive(now)) nextTick = now + tickInterval;
            end = now + duration;
        }

        public float Tick(float now)
        {
            float total = 0f;
            while (nextTick <= now + Epsilon && nextTick <= end + Epsilon)
            {
                total += damagePerTick;
                nextTick += tickInterval;
            }
            return total;
        }
    }
}
