using System;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    [Serializable]
    public struct ProductInfo
    {
        public string ProductID;
        public Sprite ProductIcon;
        public float DefaultPrice;
        public int Amount;
    }
}
