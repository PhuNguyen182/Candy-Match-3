using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Enums;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.Common.CustomConverters
{
    public class StatefulBlockPositionConverter : JsonConverter<StatefulBlockPosition>
    {
        public override StatefulBlockPosition ReadJson(JsonReader reader, Type objectType, StatefulBlockPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? x, out int? y, out int? id, out int? healthPoint, out int? groupType))
            {
                reader.Read();
            }

            if(x.HasValue && y.HasValue && id.HasValue && healthPoint.HasValue && groupType.HasValue)
            {
                return new StatefulBlockPosition
                {
                    Position = new(x.Value, y.Value),
                    ItemData = new StatefulBlockData
                    {
                        ID = id.Value,
                        HealthPoint = healthPoint.Value,
                        GroupType = (StatefulGroupType)groupType.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, StatefulBlockPosition value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Position.x);
            writer.WriteValue(value.Position.y);
            writer.WriteValue(value.ItemData.ID);
            writer.WriteValue(value.ItemData.HealthPoint);
            writer.WriteValue((int)value.ItemData.GroupType);
            writer.WriteEndArray();
        }
    }
}
