using System;

namespace InputLayer.Runtime
{
    [Flags]
    public enum InputCallback : byte
    {
        None = 0,
        Started = 1 << 0,
        Canceled = 1 << 1,
        Performed = 1 << 2,
        All = Started | Canceled | Performed
    };
}