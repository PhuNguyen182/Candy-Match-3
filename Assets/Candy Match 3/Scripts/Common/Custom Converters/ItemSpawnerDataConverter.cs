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
    public class ItemSpawnerDataConverter : JsonConverter<ItemSpawnerData>
    {
        public override ItemSpawnerData ReadJson(JsonReader reader, Type objectType, ItemSpawnerData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.ReadInts(out int? coefficient, out int? itemType))
            {
                reader.Read();
            }

            if(coefficient.HasValue && itemType.HasValue)
            {
                return new ItemSpawnerData
                {
                    DataValue = new ItemSpawner
                    {
                        Coefficient = coefficient.Value,
                        ItemType = (ItemType)itemType.Value
                    }
                };
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, ItemSpawnerData value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            writer.WriteValue(value.DataValue.Coefficient);
            writer.WriteValue((int)value.DataValue.ItemType);
            writer.WriteEndArray();
        }
    }
}
