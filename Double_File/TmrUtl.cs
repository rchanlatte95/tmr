using System.Runtime.InteropServices;

public static class TmrUtl
{
    public const float FLT_EPSILON = 0.0001f;

    // Timer structure for keeping track of time.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct timer // 8 bytes in size.
    {
        public float dur;
        public float timeRemaining;

        public timer()
        {
            dur = 0f;
            timeRemaining = 0f;
        }
    }

    // Frame Timer structure for keeping track of time in terms of frames.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct frame_timer // 8 bytes in size.
    {
        public int dur;
        public int ticksRemaining;

        public frame_timer()
        {
            dur = 0;
            ticksRemaining = 0;
        }
    }
}
