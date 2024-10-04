using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Serializable]
    public enum BackgroundMusicType
    {
        None = 0,
        Mainhome = 1,
        Gameplay = 2
    }

    [Serializable]
    public enum SoundEffectType
    {
        None = 0,

        #region Candy
        BoosterAward = 1,
        CandyFalling = 2,
        CandyMatch = 3,
        CandyWrap = 4,
        #endregion

        #region Special Items
        Chocolate = 5,
        ChocolateExpand = 6,
        Collectable = 7,
        Marshmallow = 8,
        #endregion

        #region In-game
        Error = 9,
        ComplimentText = 10,
        #endregion

        #region Stateful
        Ice = 11,
        Honey = 12,
        Syrup = 13,
        #endregion

        #region Booster
        ColorBomb = 14,
        LineVerticalHorizontal = 15,
        BoosterAppear = 16,
        ColorfulCastItems = 17,
        ColorfulFirerayCluster = 18,
        ComboBooster = 19,
        Shuffle = 20,
        #endregion

        #region Game Target
        ReachedGoal = 21,
        StarProgressBar = 22,
        #endregion

        #region UI
        AwardPopup = 23,
        Button = 24,
        BuyPopButton = 25,
        CoinsPopButton = 26,
        Lose = 27,
        NoMovesOrTimeAlert = 28,
        PopupClose = 29,
        PopupCloseButton = 30,
        PopupOpen = 31,
        PopupOpenButton = 32,
        PopupOpenWhoosh = 33,
        Rain = 34,
        Win = 35,
        WinStarPop = 36,
        #endregion

        ItemHit = 37,
        CrossLineBooster = 38,
        WrappedStriped = 39,
    }
}
