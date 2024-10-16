using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Models.SpawnRules;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SpawnItemTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;

        private MoveItemTask _moveItemTask;
        private CheckGridTask _checkGridTask;
        private Dictionary<Vector3Int, int> _spawnerPoints;
        private Dictionary<int, WeightedSpawnRule> _spawnRules;

        public SpawnItemTask(GridCellManager gridCellManager, ItemManager itemManager)
        {
            _spawnerPoints = new();
            _gridCellManager = gridCellManager;
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
            if (!_spawnerPoints.ContainsKey(position)) return;

            int id = _spawnerPoints[position];
            BlockItemPosition blockItemPosition;
            WeightedSpawnRule spawnRule = _spawnRules[id];

            BlockItemData itemData = spawnRule.GetRandomItemData(_itemManager);
            BoosterType boosterType = _itemManager.GetBoosterType(itemData.ItemType);

            if (boosterType != BoosterType.None) // Check this item is booster or not
            {
                CandyColor candyColor = itemData.ItemColor;
                byte[] boosterProperty = new byte[] { (byte)candyColor, (byte)boosterType, 0, 0 };
                int state = NumericUtils.BytesToInt(boosterProperty);

                blockItemPosition = new()
                {
                    Position = position,
                    ItemData = new BlockItemData
                    {
                        ID = 0,
                        HealthPoint = 1,
                        ItemType = itemData.ItemType,
                        ItemColor = candyColor,
                        PrimaryState = state
                    }
                };
            }

            else
            {
                blockItemPosition = new()
                {
                    Position = position,
                    ItemData = itemData
                };
            }

            IBlockItem blockItem = _itemManager.Create(blockItemPosition);
            IGridCell gridCell = _gridCellManager.Get(position);
            Vector3Int upGridPosition = position + Vector3Int.up;
            
            Vector3 upWorldPosition = _gridCellManager.ConvertGridToWorldFunction(upGridPosition);
            Vector3 spawnPosition = _gridCellManager.ConvertGridToWorldFunction(position);

            gridCell.LockStates = LockStates.Moving;
            blockItem.SetWorldPosition(upWorldPosition);
            gridCell.SetBlockItem(blockItem, false);

            if (blockItem is IItemAnimation animation)
            {
                float duration = 1f / Match3Constants.BaseItemMoveSpeed;
                await animation.MoveTo(spawnPosition, duration);
            }

            await _moveItemTask.MoveItem(gridCell);
            _moveItemTask.OnItemStopMove(gridCell);
        }

        public bool CheckSpawnable(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (!gridCell.CanContainItem)
                return false;

            if (gridCell.HasItem || gridCell.IsLocked)
                return false;

            if (!gridCell.IsSpawner)
                return false;

            return true;
        }

        public bool CheckSpawnableLite(IGridCell gridCell)
        {
            if (!gridCell.CanContainItem)
                return false;

            if (gridCell.HasItem || gridCell.IsLocked)
                return false;

            return true;
        }

        public void SetMoveItemTask(MoveItemTask moveItemTask)
        {
            _moveItemTask = moveItemTask;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            foreach (var spawnRule in _spawnRules)
            {
                spawnRule.Value.Dispose();
            }

            _spawnerPoints.Clear();
            _spawnRules.Clear();
        }
    }
}
