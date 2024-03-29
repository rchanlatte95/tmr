﻿using System.Runtime.InteropServices;

public static class Tmr
{
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

    #region Static Variables

    public const float FLT_EPSILON = 0.0001f;
    public const int MAX_TIMER_CT = 32;

    public static int activeTmrs = 0;
    public static timer[] pool = new timer[MAX_TIMER_CT];
    public static Action[] doneCallbacks = new Action[MAX_TIMER_CT];

    #endregion

    // Start a tween that will execute once and be freed automatically.
    public static int Start(float duration, Action callback)
    {
        if (activeTmrs >= MAX_TIMER_CT || callback == null) { return -1; }

        pool[activeTmrs].dur = duration;
        pool[activeTmrs].timeRemaining = duration;
        doneCallbacks[activeTmrs] = callback;

        return activeTmrs++;
    }

    public static void Stop(int id, bool exeFinishedCallback = true)
    {
        if (pool[id].timeRemaining > FLT_EPSILON) { --activeTmrs; }
        if (exeFinishedCallback) { doneCallbacks[id](); }
    }

    public static void Tick(float dt)
    {
        if (activeTmrs < 1 || MathF.Abs(dt) < FLT_EPSILON) { return; }

        for (int i = 0; i < activeTmrs; ++i)
        {
            pool[i].timeRemaining -= dt;

            if (pool[i].timeRemaining > FLT_EPSILON) { continue; }
            else
            {
                doneCallbacks[i]();
                pool[i--] = pool[--activeTmrs];
            }
        }
    }
}
