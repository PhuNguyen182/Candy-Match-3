using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(BlockItemPositionConterter))]
    public class BlockItemPosition : BaseItemPosition<BlockItemData>
    {

    }

    public struct BlockItemData
    {
        public int ID;
        public int HealthPoint;
        public ItemType ItemType;
        public int PrimaryState;
        public int SecondaryState;
    }
}
