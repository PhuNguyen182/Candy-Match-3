using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Serializable]
    public enum ItemType
    {
        None = 0,

        Blue = 1,
        BlueHorizontal = 2,
        BlueVertical = 3,
        BlueWrapped = 4,

        Green = 5,
        GreenHorizontal = 6,
        GreenVertical = 7,
        GreenWrapped = 8,

        Orange = 9,
        OrangeHorizontal = 10,
        OrangeVertical = 11,
        OrangeWrapped = 12,

        Purple = 13,
        PurpleHorizontal = 14,
        PurpleVertical = 15,
        PurpleWrapped = 16,

        Red = 17,
        RedHorizontal = 18,
        RedVertical = 19,
        RedWrapped = 20,

        Yellow = 21,
        YellowHorizontal = 22,
        YellowVertical = 23,
        YellowWrapped = 24,

        Biscuit = 25,
        Cherry = 26,
        Chocolate = 27,
        Cream = 28,
        Marshmallow = 29,
        Unbreakable = 30,
        Watermelon = 31,
        ColorBomb = 32, // Similar to rainbow entity
        Bomb = 33, // Explore 3x3

        Random = 34,
    }
}
