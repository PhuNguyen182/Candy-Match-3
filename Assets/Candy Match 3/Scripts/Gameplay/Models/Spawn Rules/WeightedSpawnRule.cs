using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomData;
using GlobalScripts.Probabilities;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.SpawnRules
{
    public class WeightedSpawnRule : IDisposable
    {
        public int ID { get; private set; }

        private List<float> _itemDistributes = new();
        private List<ItemSpawnerData> _itemSpawnerData = new();

        public void SetSpawnRuleData(SpawnRuleBlockData spawnRule)
        {
            ID = spawnRule.ID;
            _itemSpawnerData = spawnRule.ItemSpawnerData;
            SetRandomColorDistribute();
        }

        private void SetRandomColorDistribute()
        {
            List<int> itemRatios = new();

            for (int i = 0; i < _itemSpawnerData.Count; i++)
            {
                itemRatios.Add(_itemSpawnerData[i].DataValue.Coefficient);
            }

            _itemDistributes.Clear();
            for (int i = 0; i < itemRatios.Count; i++)
            {
                float itemDistribute = DistributeCalculator.GetPercentage(itemRatios[i], itemRatios);
                _itemDistributes.Add(itemDistribute);
            }
        }

        public BlockItemData GetRandomItemData(ItemManager itemManager)
        {
            int randIndex = ProbabilitiesController.GetItemByProbabilityRarity(_itemDistributes);
            ItemType itemType = _itemSpawnerData[randIndex].DataValue.ItemType;
            CandyColor candyColor = itemManager.GetColorFromItemType(itemType);
            
            return new BlockItemData
            {
                ID = 0,
                ItemType = itemType,
                ItemColor = candyColor,
                HealthPoint = 1,
            };
        }

        public void RemoveItem(ItemType itemType)
        {
            int removeIndex = -1;
            for (int i = 0; i < _itemSpawnerData.Count; i++)
            {
                if (_itemSpawnerData[i].DataValue.ItemType == itemType)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex != -1)
            {
                _itemSpawnerData.RemoveAt(removeIndex);
                SetRandomColorDistribute();
            }
        }

        public void Dispose()
        {
            ID = 0;
            _itemDistributes.Clear();
            _itemSpawnerData.Clear();
        }
    }
}
