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
    public class LevelTargetConverter : JsonConverter<LevelTargetData>
    {
        public override LevelTargetData ReadJson(JsonReader reader, Type objectType, LevelTargetData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? targetCount, out int? target))
            {
                reader.Read();
            }

            if(targetCount.HasValue && target.HasValue)
            {
                return new LevelTargetData
                {
                    DataValue = new TargetData
                    {
                        TargetAmount = targetCount.Value,
                        Target = (TargetEnum)target.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, LevelTargetData value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.DataValue.TargetAmount);
            writer.WriteValue((int)value.DataValue.Target);
            writer.WriteEndArray();
        }
    }
}
