using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Models.SpawnRules;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Strategies;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SpawnItemTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;

        private Dictionary<int, WeightedSpawnRule> _spawnRules;

        public SpawnItemTask(GridCellManager gridCellManager, ItemManager itemManager)
        {
            _itemManager = itemManager;
            _gridCellManager = gridCellManager;
        }

        public void SetItemSpawnerData(List<SpawnRuleBlockData> spawnRuleData)
        {
            _spawnRules = spawnRuleData.ToDictionary(key => key.ID, value =>
            {
                WeightedSpawnRule spawnRule = new();
                spawnRule.SetSpawnRuleData(value);
                return spawnRule;
            });
        }

        public IBlockItem Spawn(int id, Vector3Int position)
        {
            WeightedSpawnRule spawnRule = _spawnRules[id];
            BlockItemData itemData = spawnRule.GetRandomItemData(_itemManager);
            
            IBlockItem blockItem = _itemManager.Create(new BlockItemPosition
            {
                Position = position,
                ItemData = itemData
            });

            return blockItem;
        }

        public void Dispose()
        {
            foreach (var spawnRule in _spawnRules)
            {
                spawnRule.Value.Dispose();
            }

            _spawnRules.Clear();
        }
    }
}
