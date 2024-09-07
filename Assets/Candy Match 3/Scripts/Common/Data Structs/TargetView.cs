using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    public struct TargetView
    {
        public Sprite Icon;
        public TargetEnum TargetType;
    }

    public struct TargetStats
    {
        public int Amount;
        public bool IsCompleted;
        public bool IsFailed;
    }
}
