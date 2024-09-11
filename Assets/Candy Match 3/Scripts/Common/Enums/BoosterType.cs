using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Serializable]
    public enum BoosterType
    {
        None = 0,
        Normal = 1,
        Horizontal = 2,
        Vertical = 3,
        Wrapped = 4,
        Colorful = 5
    }

    public enum ComboBoosterType
    {
        None = 0,
        DoubleStriped = 1,
        StripedWrapped = 2,
        DoubleWrapped = 3,
        ColorfulStriped = 4,
        ColorfulWrapped = 5,
        DoubleColorful = 6,
    }
}
