using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.CustomData;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class BreakGridTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly MetaItemManager _metaItemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly List<Vector3Int> _adjacentStepCheck;
        
        private CheckGridTask _checkGridTask;

        public BreakGridTask(GridCellManager gridCellManager, MetaItemManager metaItemManager, ItemManager itemManager)
        {
            _gridCellManager = gridCellManager;
            _metaItemManager = metaItemManager;
            _itemManager = itemManager;

            _adjacentStepCheck = new()
            {
                new(1, 0), new(0, 1), new(-1, 0), new(0, -1)
            };
        }

        public async UniTask Break(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);
            gridCell.LockStates = LockStates.Breaking;
            await Break(gridCell);
        }

        public async UniTask<bool> Break(IGridCell gridCell)
        {
            if (gridCell == null)
                return false;

            if (!gridCell.HasItem)
                return false;

            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                await booster.Activate(); // use activate booster task later
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
                IBlockItem blockItem = gridCell.BlockItem;
                if (blockItem is IBreakable breakable)
                {
                    if (breakable.Break())
                    {
                        await blockItem.ItemBlast();
                        ReleaseGridCell(gridCell);
                    }
                }
            }

            ItemType booster = _itemManager.GetBoosterTypeFromMatch(matchType, candyColor);

            _itemManager.Add(new BlockItemPosition
            {
                Position = gridCell.GridPosition,
                ItemData = new BlockItemData
                {
                    ID = 0,
                    HealthPoint = 1,
                    ItemType = booster,
                    ItemColor = candyColor,
                }
            });
        }

        public async UniTask BreakMatchItem(IGridCell gridCell, int matchCount, MatchType matchType)
        {
            if (!gridCell.HasItem)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if(blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    // To do: do different effect
                    await blockItem.ItemBlast();
                    ReleaseGridCell(gridCell);
                }
            }

            gridCell.LockStates = LockStates.None;
        }

        public async UniTask BreakMatch(MatchResult matchResult)
        {
            using (var listPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
            {
                for (int i = 0; i < matchResult.MatchSequence.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(matchResult.MatchSequence[i]);

                    gridCell.LockStates = LockStates.Matching;
                    breakTasks.Add(Break(gridCell));
                }

                await UniTask.WhenAll(breakTasks);
            }
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

        private void ReleaseGridCell(IGridCell gridCell)
        {
            gridCell.ReleaseGrid();
            _metaItemManager.ReleaseGridCell(gridCell.GridPosition);
        }

        public void Dispose()
        {
            _adjacentStepCheck.Clear();
        }
    }
}
