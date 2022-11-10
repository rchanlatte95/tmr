using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct state // 4 bytes in size.
{
    [FieldOffset(0)] public int full;
    [FieldOffset(0)] public byte RESERVED;
    [FieldOffset(1)] public byte RUNNING;
    [FieldOffset(2)] public byte ID;
    [FieldOffset(3)] public byte TBD; // free. can use at your discretion
}

// Timer structure for keeping track of time.
[StructLayout(LayoutKind.Sequential)]
public struct timer // 12 bytes
{
    public float dur;
    public float remaining;
    public state state;
}

public static class Tmr
{
    public const byte FALSE = 0;
    public const byte TRUE = 1;

    // logging variables to keep track of the overall tween system
    public const float FLT_EPSILON = 0.001f;
    public static int reservedTmrs = 0;
    public static int currActiveTmrs = 0;

    public const int MAX_TIMER_CT = 16;
    public static timer[] pool = new timer[MAX_TIMER_CT];
    
    public static Action[] doneCallbacks = new Action[MAX_TIMER_CT];

    // Stop the Timers from executing 
    public static void FlushPool(bool stopExe = false)
    {
        if(stopExe == false)
        {
            for (int i = 0; i < MAX_TIMER_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE)
                {
                    pool[i].state.RESERVED = FALSE;

                    if (pool[i].state.RUNNING == TRUE)
                    {
                        pool[i].state.RUNNING = FALSE;
                        --currActiveTmrs;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < MAX_TIMER_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE)
                {
                    pool[i].state.RESERVED = FALSE;
                }
            }
        }
    }

    // Reserves a timer and returns the index to the timer in the pool.
    // 
    // A return value of -1 indicates that there were no more tweens to be reserved.
    public static int Reserve()
    {
        if (reservedTmrs < MAX_TIMER_CT)
        {
            for (int i = 0; i < MAX_TIMER_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE) { continue; }
                else
                {
                    pool[i].state.RESERVED = TRUE;
                    ++reservedTmrs;
                    return i;
                }
            }
        }

        return -1;
    }

    // "Frees" a reserved timer previously reserved and returns it to the pool.
    public static void Free(int id)
    {
        bool idInRange = id < MAX_TIMER_CT & id > -1;
        if (idInRange && pool[id].state.RESERVED == TRUE)
        {
            pool[id].state.RESERVED = FALSE;
            --reservedTmrs;
        }
    }

    // Reserves AND Initializes a timer. 
    // Returns the Index of the timer reserved for you.
    public static int ResInit(float duration, Action endCallback)
    {
        if (reservedTmrs < MAX_TIMER_CT)
        {
            for (int i = 0; i < MAX_TIMER_CT; ++i)
            {
                if (pool[i].state.RESERVED == TRUE) { continue; }
                else
                {
                    pool[i].state.RESERVED = TRUE;
                    pool[i].dur = duration;
                    doneCallbacks[i] = endCallback;
                    ++reservedTmrs;
                    return i;
                }
            }
        }

        return -1;
    }

    // Initializes a reserved timer.
    public static void Init(int id, float duration, Action endCallback)
    {
        pool[id].dur = duration;
        doneCallbacks[id] = endCallback;
    }

    // Start execution of timer given some arbitrary duration and completion function.
    public static void Start(int id, float duration, Action endCallback)
    {
        pool[id].dur = duration;
        pool[id].remaining = duration;
        doneCallbacks[id] = endCallback;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTmrs;
        }
    }

    // Start execution of timer given some arbitrary duration.
    public static void Start(int id, float duration)
    {
        pool[id].dur = duration;
        pool[id].remaining = duration;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTmrs;
        }
    }

    // Enable execution of the passed timer without modifying it.
    public static void Start(int id)
    {
        pool[id].remaining = pool[id].dur;

        if (pool[id].state.RUNNING == FALSE)
        {
            pool[id].state.RUNNING = TRUE;
            ++currActiveTmrs;
        }
    }

    // Stop a timer from ticking. Can execute finished callback if desired.
    public static void Stop(int id, bool exeFinishedCallback = true)
    {
        if (pool[id].state.RUNNING == TRUE)
        {
            pool[id].state.RUNNING = FALSE;
            --currActiveTmrs;
        }

        if (doneCallbacks[id] != null && exeFinishedCallback) { doneCallbacks[id](); }
    }

    // Increment/Tick all timers.
    public static void Tick(float dt)
    {
        int tmrs2Update = currActiveTmrs;
        for (int i = 0; tmrs2Update > 0; ++i)
        {
            if (pool[i].state.RUNNING == FALSE) { continue; }
            else
            {
                --tmrs2Update;
                pool[i].remaining -= dt;

                if (pool[i].remaining > FLT_EPSILON) { continue; }
                else
                {
                    pool[i].state.RUNNING = FALSE;
                    --currActiveTmrs;
                    doneCallbacks[i]();
                }
            }
        }
    }
}
