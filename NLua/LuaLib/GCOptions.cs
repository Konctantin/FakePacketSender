namespace NLua
{
    public enum GCOptions : int
    {
        /// <summary>
        /// Stops the garbage collector.
        /// </summary>
        Stop = 0,

        /// <summary>
        /// Restarts the garbage collector.
        /// </summary>
        Restart = 1,

        /// <summary>
        /// Performs a full garbage-collection cycle.
        /// </summary>
        Collect = 2,

        /// <summary>
        /// Returns the current amount of memory (in Kbytes) in use by KopiLua.Lua.
        /// </summary>
        Count = 3,

        /// <summary>
        /// Returns the remainder of dividing the current amount of bytes of memory in use by Lua by 1024.
        /// </summary>
        CountB = 4,

        /// <summary>
        /// Performs an incremental step of garbage collection. The step "size" is controlled by data (larger values mean more steps) in a non-specified way. ifyou want to control the step size you must experimentally tune the value of data. The function returns 1 ifthe step finished a garbage-collection cycle.
        /// </summary>
        Step = 5,

        /// <summary>
        /// Sets data as the new value for the pause (Controls how long the collector waits before starting a new cycle) of the collector (see §2.10). The function returns the previous value of the pause.
        /// </summary>
        SetPause = 6,

        /// <summary>
        /// Sets data as the new value for the step multiplier of the collector (Controls the relative speed of the collector relative to memory allocation.). The function returns the previous value of the step multiplier.
        /// </summary>
        SetStepMul = 7
    }
}