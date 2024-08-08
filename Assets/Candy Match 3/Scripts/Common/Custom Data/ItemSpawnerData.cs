using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(ItemSpawnerDataConverter))]
    public class ItemSpawnerData : BaseCustomData<ItemSpawner>
    {

    }

    public struct ItemSpawner
    {
        public int Coefficient;
        public ItemType ItemType;
    }
}
