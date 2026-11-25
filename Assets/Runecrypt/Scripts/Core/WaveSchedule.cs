using System;

namespace Runecrypt.Core
{
    // Gelombang per ruangan (spec M2.6 §5.2): 3 gelombang per ruangan, gelombang berikutnya saat sisa musuh ≤ 4,
    // ruangan berikutnya saat arena kosong, batas 12 musuh hidup dan 8 per gelombang.
    public class WaveSchedule
    {
        readonly int wavesPerRoom, nextWaveAtAlive, maxAlive, maxPerWave;
        int lastSize; // ukuran gelombang yang terakhir dimulai

        public int Wave { get; private set; }   // 0 = belum mulai
        public int Room => Wave == 0 ? 0 : (Wave - 1) / wavesPerRoom + 1;
        public int WaveInRoom => Wave == 0 ? 0 : (Wave - 1) % wavesPerRoom + 1;
        public bool IsRoomStart => WaveInRoom == 1;
        bool LastInRoom => WaveInRoom == wavesPerRoom;

        public WaveSchedule(int wavesPerRoom = 3, int nextWaveAtAlive = 4, int maxAlive = 12, int maxPerWave = 8)
        {
            this.wavesPerRoom = wavesPerRoom;
            this.nextWaveAtAlive = nextWaveAtAlive;
            this.maxAlive = maxAlive;
            this.maxPerWave = maxPerWave;
        }

        // Ukuran gelombang berikutnya: naik satu per gelombang dan per ruangan; ruangan 1 = 3 + 4 + 5.
        public int NextWaveSize()
        {
            int next = Wave + 1;
            int room = (next - 1) / wavesPerRoom + 1, inRoom = (next - 1) % wavesPerRoom + 1;
            return Math.Min(1 + room + inRoom, maxPerWave);
        }

        public bool ShouldStartNextWave(int alive, int pending)
        {
            if (pending > 0) return false;
            if (Wave == 0 || LastInRoom) return alive == 0;
            return alive <= Math.Min(nextWaveAtAlive, lastSize / 2); // gelombang kecil tidak langsung disusul
        }

        public int CanSpawnNow(int alive) => Math.Max(0, maxAlive - alive);

        public void AdvanceWave()
        {
            lastSize = NextWaveSize();
            Wave++;
        }

        public bool RoomDone(int alive, int pending) => Wave > 0 && LastInRoom && alive == 0 && pending == 0;
    }
}
