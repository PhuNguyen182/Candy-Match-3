using System;

namespace CandyMatch3.Scripts.Common.Enums
{
    [Serializable]
    public enum SoundEffectType
    {
        None = 0,

        #region Candy
        BoosterAward,
        CandyFalling,
        CandyMatch,
        CandyWrap,
        #endregion

        #region Special Items
        Chocolate,
        ChocolateExpand,
        Collectable,
        Marshmallow,
        #endregion

        #region In-game
        Error,
        ComplimentText,
        #endregion

        #region Stateful
        Ice,
        Honey,
        Syrup,
        #endregion

        #region Booster
        ColorBomb,
        LineVerticalHorizontal,
        #endregion

        #region Game Target
        ReachedGoal,
        StarProgressBar,
        #endregion

        #region UI
        AwardPopup,
        Button,
        BuyPopButton,
        CoinsPopButton,
        Lose,
        NoMovesOrTimeAlert,
        PopupClose,
        PopupCloseButton,
        PopupOpen,
        PopupOpenButton,
        PopupOpenWhoosh,
        Rain,
        Win,
        WinStarPop
        #endregion
    }
}
