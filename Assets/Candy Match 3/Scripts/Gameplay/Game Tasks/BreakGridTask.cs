using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.CustomData;
using Cysharp.Threading.Tasks;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class BreakGridTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly MetaItemManager _metaItemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentStepCheck;
        
        private CheckGridTask _checkGridTask;
        private ActivateBoosterTask _activateBoosterTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public BreakGridTask(GridCellManager gridCellManager, MetaItemManager metaItemManager, ItemManager itemManager)
        {
            _gridCellManager = gridCellManager;
            _metaItemManager = metaItemManager;
            _itemManager = itemManager;

            _adjacentStepCheck = new()
            {
                new(1, 0), new(0, 1), new(-1, 0), new(0, -1)
            };

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Break(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);
            await Break(gridCell);
        }

        public async UniTask BreakItem(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if(gridCell != null && gridCell.HasItem)
            {
                gridCell.LockStates = LockStates.Breaking;
                IBlockItem blockItem = gridCell.BlockItem;

                if (blockItem is IBooster booster)
                {
                    await booster.Activate();
                    await _activateBoosterTask.ActivateBooster(gridCell);
                    ReleaseGridCell(gridCell);
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                if (blockItem is IBreakable breakable)
                {
                    if (breakable.Break())
                    {
                        await blockItem.ItemBlast();
                        await _activateBoosterTask.ActivateBooster(gridCell);
                        ReleaseGridCell(gridCell);
                    }
                }

                gridCell.LockStates = LockStates.None;
            }
        }

        public async UniTask<bool> Break(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (!gridCell.HasItem)
                return false;

            gridCell.LockStates = LockStates.Breaking;
            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                await booster.Activate(); // use activate booster task later
                await _activateBoosterTask.ActivateBooster(gridCell);
                ReleaseGridCell(gridCell);
                return true;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();
                    ReleaseGridCell(gridCell);

                    _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);
                    gridCell.LockStates = LockStates.None;
                    return true;
                }
            }

            return false;
        }

        public async UniTask SpawnBooster(IGridCell gridCell, MatchType matchType, CandyColor candyColor)
        {
            if (gridCell.HasItem)
            {
                gridCell.LockStates = LockStates.Replacing;
                IBlockItem blockItem = gridCell.BlockItem;

                if (blockItem is IBooster booster)
                {
                    await booster.Activate(); // use activate booster task later
                    await _activateBoosterTask.ActivateBooster(gridCell);
                    ReleaseGridCell(gridCell);
                }

                if (blockItem is IBreakable breakable)
                {
                    if (breakable.Break())
                    {
                        //await blockItem.ItemBlast();
                        await _activateBoosterTask.ActivateBooster(gridCell);
                        ReleaseGridCell(gridCell);
                    }
                }

                (ItemType itemType, ColorBoosterType boosterType) = _itemManager.GetBoosterTypeFromMatch(matchType, candyColor);
                byte[] boosterProperty = new byte[] { (byte)candyColor, (byte)boosterType, 0, 0 };

                _itemManager.Add(new BlockItemPosition
                {
                    Position = gridCell.GridPosition,
                    ItemData = new BlockItemData
                    {
                        ID = 0,
                        HealthPoint = 1,
                        ItemType = itemType,
                        ItemColor = candyColor,
                        PrimaryState = NumericUtils.BytesToInt(boosterProperty)
                    }
                });

                gridCell.LockStates = LockStates.None;
            }
        }

        public async UniTask BreakMatchItem(IGridCell gridCell, int matchCount)
        {
            if (!gridCell.HasItem)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if(blockItem is IBooster booster)
            {
                await booster.Activate(); // Do logic active booster later
                await _activateBoosterTask.ActivateBooster(gridCell);
                ReleaseGridCell(gridCell);
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();
                    ReleaseGridCell(gridCell);
                }
            }

            gridCell.LockStates = LockStates.None;
        }

        public async UniTask BreakAdjacent(IGridCell gridCell)
        {
            if (gridCell == null || !gridCell.HasItem)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            if (blockItem is IAdjcentBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();
                    ReleaseGridCell(gridCell);
                }
            }
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void SetActivateBoosterTask(ActivateBoosterTask activateBoosterTask)
        {
            _activateBoosterTask = activateBoosterTask;
        }

        public void ReleaseGridCell(IGridCell gridCell)
        {
            gridCell.ReleaseGrid();
            _metaItemManager.ReleaseGridCell(gridCell.GridPosition);
        }

        public void Dispose()
        {
            _cts.Dispose();
            _adjacentStepCheck.Clear();
        }
    }
}
