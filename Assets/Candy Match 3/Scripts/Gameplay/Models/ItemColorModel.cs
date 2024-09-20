using System;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models
{
    [Serializable]
    public class ItemColorModel
    {
        public CandyColor CandyColor;
        public BoosterType ColorBoosterType;
        public ItemType ItemType;
    }
}
