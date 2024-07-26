using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.CustomData;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class SpawnerBlockPositionConverter : JsonConverter<SpawnerBlockPosition>
    {
        public override SpawnerBlockPosition ReadJson(JsonReader reader, Type objectType, SpawnerBlockPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? x, out int? y, out int? id))
            {
                reader.Read();
            }

            if(x.HasValue && y.HasValue && id.HasValue)
            {
                return new SpawnerBlockPosition
                {
                    Position = new(x.Value, y.Value),
                    ItemData = new SpawnerBlockData
                    {
                        ID = id.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, SpawnerBlockPosition value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Position.x);
            writer.WriteValue(value.Position.y);
            writer.WriteValue(value.ItemData.ID);
            writer.WriteEndArray();
        }
    }
}
