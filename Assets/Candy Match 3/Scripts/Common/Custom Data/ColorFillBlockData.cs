using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomConverters;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [JsonConverter(typeof(ColorFillBlockConverter))]
    public class ColorFillBlockData : BaseCustomData<ColorFillData>
    {

    }

    public struct ColorFillData
    {
        public int Coefficient;
        public CandyColor Color;
    }
}
