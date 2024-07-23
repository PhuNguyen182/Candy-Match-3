using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(SpawnerBlockPositionConverter))]
    public class SpawnerBlockPosition : BaseBlockPosition<SpawnerBlockData>
    {

    }

    public struct SpawnerBlockData
    {
        public int ID;
    }
}
