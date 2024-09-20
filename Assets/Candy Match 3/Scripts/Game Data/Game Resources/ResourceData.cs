using System;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.GameData.GameResources
{
    [Serializable]
    public struct ResourceData
    {
        public GameResourceType ID;
        public int Amount;
    }
}
