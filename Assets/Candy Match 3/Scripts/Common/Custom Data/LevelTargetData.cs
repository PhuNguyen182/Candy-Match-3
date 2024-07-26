using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(LevelTargetConverter))]
    public class LevelTargetData : BaseCustomData<TargetData>
    {

    }

    public struct TargetData
    {
        public int TargetAmount;
        public TargetEnum Target;
    }
}
