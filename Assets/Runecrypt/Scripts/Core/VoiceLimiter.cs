namespace Runecrypt.Core
{
    // Memilih slot AudioSource: batas per klip dulu, lalu slot kosong, lalu curi yang tertua.
    // Klip yang sama yang mulai lagi dalam 30 ms dilewati (-1): salinan serempak menumpuk koheren dan clipping.
    public class VoiceLimiter
    {
        const float SameClipGap = 0.03f;
        readonly int perClip;
        readonly int[] clip;
        readonly float[] start, end;

        public VoiceLimiter(int slots = 12, int perClip = 4)
        {
            this.perClip = perClip;
            clip = new int[slots];
            start = new float[slots];
            end = new float[slots];
            for (int i = 0; i < slots; i++) end[i] = float.NegativeInfinity;
        }

        public int Acquire(int clipId, float now, float length)
        {
            for (int i = 0; i < clip.Length; i++)
            {
                bool used = end[i] > start[i]; // slot yang belum pernah dipakai punya end = -∞
                if (used && clip[i] == clipId && now - start[i] < SameClipGap) return -1;
            }
            int slot = PickSlot(clipId, now);
            clip[slot] = clipId;
            start[slot] = now;
            end[slot] = now + length;
            return slot;
        }

        int PickSlot(int clipId, float now)
        {
            int sameCount = 0, oldestSame = -1, free = -1, oldest = 0;
            for (int i = 0; i < clip.Length; i++)
            {
                bool active = end[i] > now;
                if (active && clip[i] == clipId)
                {
                    sameCount++;
                    if (oldestSame < 0 || start[i] < start[oldestSame]) oldestSame = i;
                }
                if (!active && free < 0) free = i;
                if (start[i] < start[oldest]) oldest = i;
            }
            if (sameCount >= perClip) return oldestSame;
            return free >= 0 ? free : oldest;
        }
    }
}
