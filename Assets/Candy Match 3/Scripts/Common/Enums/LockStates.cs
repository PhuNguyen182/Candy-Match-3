using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Flags]
    public enum LockStates
    {
        None = 0,
        Moving = 1 << 1,
        Breaking = 1 << 2,
        Replacing = 1 << 3,
        Matching = 1 << 4,
        Swapping = 1 << 5,
        Exiting = 1 << 6
    }
}
