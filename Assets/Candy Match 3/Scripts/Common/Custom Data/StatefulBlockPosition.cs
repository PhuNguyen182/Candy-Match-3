using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(StatefulBlockPositionConverter))]
    public class StatefulBlockPosition : BaseBlockPosition<StatefulBlockData>
    {

    }

    public struct StatefulBlockData
    {
        public int ID;
        public int HealthPoint;
        public StatefulGroupType GroupType;
    }
}
