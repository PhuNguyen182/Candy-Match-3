using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomConverters;
using CandyMatch3.Scripts.GameData.LevelProgresses;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(LevelProgressNodeDataConverter))]
    public class LevelProgressNodeData : BaseCustomData<LevelProgressNode>
    {

    }
}
