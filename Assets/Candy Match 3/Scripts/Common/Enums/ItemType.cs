using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Serializable]
    public enum ItemType
    {
        None = 0,
        Random = 1,

        #region Colored Item
        Blue = 2,
        BlueHorizontal = 3,
        BlueVertical = 4,
        BlueWrapped = 5,

        Green = 6,
        GreenHorizontal = 7,
        GreenVertical = 8,
        GreenWrapped = 9,

        Orange = 10,
        OrangeHorizontal = 11,
        OrangeVertical = 12,
        OrangeWrapped = 13,

        Purple = 14,
        PurpleHorizontal = 15,
        PurpleVertical = 16,
        PurpleWrapped = 17,

        Red = 18,
        RedHorizontal = 19,
        RedVertical = 20,
        RedWrapped = 21,

        Yellow = 22,
        YellowHorizontal = 23,
        YellowVertical = 24,
        YellowWrapped = 25,
        #endregion

        // Original
        #region Speccial Item
        Biscuit = 26,
        Cherry = 27,
        Chocolate = 28,
        Cream = 29,
        Marshmallow = 30,
        Unbreakable = 31,
        Watermelon = 32,
        #endregion

        #region Booster
        ColorBomb = 33, // Similar to rainbow entity
        Bomb = 34, // Explore 3x3
        #endregion
    }
}
