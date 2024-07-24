using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class ColorFillBlockConverter : JsonConverter<ColorFillBlockData>
    {
        public override ColorFillBlockData ReadJson(JsonReader reader, Type objectType, ColorFillBlockData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? coefficient, out int? color))
            {
                reader.Read();
            }

            if(coefficient.HasValue && color.HasValue)
            {
                return new ColorFillBlockData
                {
                    DataValue = new ColorFillData
                    {
                        Coefficient = coefficient.Value,
                        Color = (CandyColor)color.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ColorFillBlockData value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.DataValue.Coefficient);
            writer.WriteValue((int)value.DataValue.Color);
            writer.WriteEndArray();
        }
    }
}
