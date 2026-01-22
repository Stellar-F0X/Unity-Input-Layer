using System;

namespace LayeredInputSystem.Runtime
{
    [Flags]
    public enum InputCallbackFlags : byte
    {
        None = 0,
        Started = 1 << 0,
        Canceled = 1 << 1,
        Performed = 1 << 2,
        All = Started | Canceled | Performed
    };
}