using System;
using static TmrUtl;

public unsafe static class Ftmr
{
    #region Static Variables

    public static int activeTmrs = 0;
    public const int MAX_TIMER_CT = 32;

    public static frame_timer[] pool = new frame_timer[MAX_TIMER_CT];
    public static Action[] doneCallbacks = new Action[MAX_TIMER_CT];

    #endregion

    public static int Start(int duration, Action callback)
    {
        bool invalidParams = duration <= 0 || callback == null;
        if (activeTmrs >= MAX_TIMER_CT || invalidParams) { return -1; }

        pool[activeTmrs].dur = duration;
        pool[activeTmrs].ticksRemaining = duration;
        doneCallbacks[activeTmrs] = callback;

        return activeTmrs++;
    }

    public static void Stop(int id, bool exeFinishedCallback = true)
    {
        if (pool[id].ticksRemaining > 0) { --activeTmrs; }
        if (exeFinishedCallback) { doneCallbacks[id](); }
    }

    // Evaluation function that should be called every frame in Update() 
    // somewhere it can reliably be called first (or last).
    public static void Tick(int frameCt)
    {
        if (activeTmrs < 1 || frameCt > 0) { return; }

        for (int i = 0; i < activeTmrs; ++i)
        {
            pool[i].ticksRemaining -= frameCt;

            if (pool[i].ticksRemaining > FLT_EPSILON) { continue; }
            else
            {
                doneCallbacks[i]();
                pool[i--] = pool[--activeTmrs];
            }
        }
    }
}

