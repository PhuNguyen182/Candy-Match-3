using System;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    [Serializable]
    public struct InGameBoosterPack
    {
        public InGameBoosterType BoosterType;
        public int Price;
        public int Amount;
    }
}
