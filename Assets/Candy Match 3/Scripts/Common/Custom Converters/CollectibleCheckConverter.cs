using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.CustomData;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class CollectibleCheckConverter : JsonConverter<CollectibleCheckBlockPosition>
    {
        public override CollectibleCheckBlockPosition ReadJson(JsonReader reader, Type objectType, CollectibleCheckBlockPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? x, out int? y))
            {
                reader.Read();
            }

            if(x.HasValue && y.HasValue)
            {
                return new CollectibleCheckBlockPosition
                {
                    Position = new(x.Value, y.Value)
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, CollectibleCheckBlockPosition value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Position.x);
            writer.WriteValue(value.Position.y);
            writer.WriteEndArray();
        }
    }
}
