using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.CustomData;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class BoardBlockPositionConverter : JsonConverter<BoardBlockPosition>
    {
        public override BoardBlockPosition ReadJson(JsonReader reader, Type objectType, BoardBlockPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? x, out int? y))
            {
                reader.Read();
            }

            if(x.HasValue && y.HasValue)
            {
                return new BoardBlockPosition
                {
                    Position = new(x.Value, y.Value)
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, BoardBlockPosition value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Position.x);
            writer.WriteValue(value.Position.y);
            writer.WriteEndArray();
        }
    }
}
