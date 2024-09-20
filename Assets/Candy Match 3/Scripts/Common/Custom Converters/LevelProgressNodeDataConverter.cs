using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.GameData.LevelProgresses;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class LevelProgressNodeDataConverter : JsonConverter<LevelProgressNodeData>
    {
        public override LevelProgressNodeData ReadJson(JsonReader reader, Type objectType, LevelProgressNodeData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? level, out int? stars))
            {
                reader.Read();
            }

            if(level.HasValue && stars.HasValue)
            {
                return new LevelProgressNodeData
                {
                    DataValue = new LevelProgressNode
                    {
                        Level = level.Value,
                        Stars = stars.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, LevelProgressNodeData value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.DataValue.Level);
            writer.WriteValue(value.DataValue.Stars);
            writer.WriteEndArray();
        }
    }
}
