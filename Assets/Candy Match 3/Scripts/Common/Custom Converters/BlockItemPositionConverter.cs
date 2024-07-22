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
    public class BlockItemPositionConverter : JsonConverter<BlockItemPosition>
    {
        public override BlockItemPosition ReadJson(JsonReader reader, Type objectType, BlockItemPosition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? x, out int? y, out int? id, out int? healthPoint, out int? itemType, out int? primaryState, out int? secondaryState))
            {
                reader.Read();
            }

            if(x.HasValue && y.HasValue && id.HasValue && healthPoint.HasValue && itemType.HasValue && primaryState.HasValue && secondaryState.HasValue)
            {
                return new BlockItemPosition
                {
                    Position = new(x.Value, y.Value),
                    ItemData = new BlockItemData
                    {
                        ID = id.Value,
                        HealthPoint = healthPoint.Value,
                        ItemType = (ItemType)itemType.Value,
                        PrimaryState = primaryState.Value,
                        SecondaryState = secondaryState.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, BlockItemPosition value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.Position.x);
            writer.WriteValue(value.Position.y);
            writer.WriteValue(value.ItemData.ID);
            writer.WriteValue((int)value.ItemData.ItemType);
            writer.WriteValue(value.ItemData.PrimaryState);
            writer.WriteValue(value.ItemData.SecondaryState);
            writer.WriteEndArray();
        }
    }
}
