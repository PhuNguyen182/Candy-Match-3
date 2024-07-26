using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(BoardBlockPositionConverter))]
    public class BoardBlockPosition : BaseBlockPosition<BoardData>
    {

    }

    public struct BoardData { }
}
