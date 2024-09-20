using System;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    [Serializable]
    public struct TargetDisplayData
    {
        public TargetEnum Target;
        [PreviewField(60)]
        public Sprite Icon;
    }
}
