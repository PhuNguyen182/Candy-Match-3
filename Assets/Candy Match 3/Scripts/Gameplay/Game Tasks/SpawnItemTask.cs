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
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Constants;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SpawnItemTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly MatchItemsTask _matchItemsTask;

        private Dictionary<Vector3Int, int> _spawnerPoints;
        private Dictionary<int, WeightedSpawnRule> _spawnRules;

        public SpawnItemTask(GridCellManager gridCellManager, MatchItemsTask matchItemsTask, ItemManager itemManager)
        {
            _spawnerPoints = new();
            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;
            _itemManager = itemManager;
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

        public void AddSpawnerPosition(SpawnerBlockPosition spawnerPosition)
        {
            _spawnerPoints.Add(spawnerPosition.Position, spawnerPosition.ItemData.ID);
        }

        public async UniTask Spawn(Vector3Int position)
        {
            if (!_spawnerPoints.ContainsKey(position))
                return;

            int id = _spawnerPoints[position];
            WeightedSpawnRule spawnRule = _spawnRules[id];
            BlockItemData itemData = spawnRule.GetRandomItemData(_itemManager);
            
            IBlockItem blockItem = _itemManager.Create(new BlockItemPosition
            {
                Position = position,
                ItemData = itemData
            });

            IGridCell gridCell = _gridCellManager.Get(position);
            Vector3Int upGridPosition = position + Vector3Int.up;
            
            Vector3 upWorldPosition = _gridCellManager.ConvertGridToWorldFunction(upGridPosition);
            Vector3 spawnPosition = _gridCellManager.ConvertGridToWorldFunction(position);
            
            blockItem.SetWorldPosition(upWorldPosition);
            gridCell.SetBlockItem(blockItem);

            if (blockItem is IItemAnimation animation)
            {
                float duration = 1f / Match3Constants.BaseItemMoveSpeed;
                await animation.MoveTo(spawnPosition, duration);
            }
        }

        public bool CheckSpawnable(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;
            
            if (!gridCell.IsSpawner)
                return false;

            if (gridCell.HasItem)
                return false;

            if (gridCell.IsMoveable)
                return false;

            return true;
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
