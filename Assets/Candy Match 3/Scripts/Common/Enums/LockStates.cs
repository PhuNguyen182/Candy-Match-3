using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Flags]
    public enum LockStates
    {
        None = 0,
        Idling = 1 << 1,
        Moving = 1 << 2,
        Breaking = 1 << 3,
        Replacing = 1 << 4,
        Matching = 1 << 5,
        Swapping = 1 << 6,
        Exiting = 1 << 7
    }
}
