using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using CandyMatch3.Scripts.Common.Constants;
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

            if (gridCell == null)
                return;

            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                if (stateBreakable.Break())
                {
                    _checkGridTask.CheckMatchAtPosition(position);
                    gridCell.SetGridStateful(new AvailableState());
                }

                _checkGridTask.CheckAroundPosition(position, 1);
                return;
            }

            if (gridCell.HasItem)
            {
                if (gridCell.LockStates == LockStates.Preparing || gridCell.LockStates == LockStates.None)
                {
                    gridCell.LockStates = LockStates.Breaking;
                    IBlockItem blockItem = gridCell.BlockItem;

                    if (blockItem is IBooster booster)
                    {
                        if (booster.IsNewCreated)
                        {
                            gridCell.IsMatching = false;
                            gridCell.LockStates = LockStates.None;
                            return;
                        }

                        await _activateBoosterTask.ActivateBooster(gridCell, true, false);
                        gridCell.LockStates = LockStates.None;
                        return;
                    }

                    if (blockItem is IBreakable breakable)
                    {
                        if (breakable.Break())
                        {
                            blockItem.ItemBlast().Forget();

                            if (blockItem is IItemEffect effect)
                                effect.PlayMatchEffect();

                            ReleaseGridCell(gridCell);
                        }
                    }
                }
            }

            gridCell.LockStates = LockStates.None;
        }

        public async UniTask Break(IGridCell gridCell)
        {
            if (gridCell == null)
                return;

            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                Vector3Int position = gridCell.GridPosition;
                if (stateBreakable.Break())
                {
                    _checkGridTask.CheckMatchAtPosition(position);
                    gridCell.SetGridStateful(new AvailableState());
                }

                _checkGridTask.CheckAroundPosition(position, 1);
                return;
            }

            if (!gridCell.HasItem || gridCell.IsLocked)
                return;

            gridCell.LockStates = LockStates.Breaking;
            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                if (booster.IsNewCreated)
                {
                    gridCell.IsMatching = false;
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                await _activateBoosterTask.ActivateBooster(gridCell, true, false);
                gridCell.LockStates = LockStates.None;
                return;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();

                    if (blockItem is IItemEffect effect)
                        effect.PlayMatchEffect();

                    ReleaseGridCell(gridCell);
                    gridCell.LockStates = LockStates.None;
                    _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);
                }
            }
        }

        public async UniTask AddBooster(IGridCell gridCell, MatchType matchType, CandyColor candyColor, Action<BoundsInt> onActive = null)
        {
            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                bool isLockedState = gridCell.GridStateful.IsLocked;

                if (stateBreakable.Break())
                {
                    gridCell.SetGridStateful(new AvailableState());
                }

                if (isLockedState)
                    return;
            }

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if (gridCell.HasItem)
            {
                if (blockItem is IBooster booster)
                {
                    if (booster.IsNewCreated)
                    {
                        gridCell.IsMatching = false;
                        gridCell.LockStates = LockStates.None;
                        return;
                    }

                    await _activateBoosterTask.ActivateBooster(gridCell, true, true, onActive);
                }

                else if (blockItem is IBreakable breakable)
                {
                    if (breakable.Break())
                    {
                        ReleaseGridCell(gridCell);
                    }
                }
            }

            var (itemType, boosterType) = _itemManager.GetBoosterTypeFromMatch(matchType, candyColor);
            byte[] boosterProperty = new byte[] { (byte)candyColor, (byte)boosterType, 0, 0 };
            int state = NumericUtils.BytesToInt(boosterProperty);

            _itemManager.Add(new BlockItemPosition
            {
                Position = gridCell.GridPosition,
                ItemData = new BlockItemData
                {
                    ID = 0,
                    HealthPoint = 1,
                    ItemType = itemType,
                    ItemColor = candyColor,
                    PrimaryState = state
                }
            });

            if (gridCell.BlockItem is IItemEffect effect)
                effect.PlayStartEffect();

            TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);

            gridCell.IsMatching = false;
            gridCell.LockStates = LockStates.None;
        }

        public async UniTask BreakMatchItem(IGridCell gridCell, int matchCount, Action<BoundsInt> onActive = null)
        {
            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                Vector3Int position = gridCell.GridPosition;
                bool isLockedState = gridCell.GridStateful.IsLocked;

                if (stateBreakable.Break())
                {
                    gridCell.SetGridStateful(new AvailableState());
                }

                if (isLockedState)
                    return;
            }

            if (gridCell.IsLocked)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if (blockItem is IBooster booster)
            {
                if (booster.IsNewCreated)
                {
                    gridCell.IsMatching = false;
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                await _activateBoosterTask.ActivateBooster(gridCell, true, true, onActive);
                
                gridCell.IsMatching = false;
                gridCell.LockStates = LockStates.None;
                return;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();

                    if (blockItem is IItemEffect effect)
                        effect.PlayMatchEffect();

                    ReleaseGridCell(gridCell);
                }
            }

            gridCell.IsMatching = false;
            gridCell.LockStates = LockStates.None;
        }

        public async UniTask BreakAdjacent(IGridCell gridCell)
        {
            if (gridCell == null || !gridCell.HasItem || gridCell.IsLocked)
                return;

            if (gridCell.GridStateful is IAdjcentBreakable stateBreakable)
            {
                if (stateBreakable.Break())
                {
                    gridCell.SetGridStateful(new AvailableState());
                }

                Vector3Int position = gridCell.GridPosition;
                _checkGridTask.CheckAroundPosition(position, 1);
                return;
            }

            gridCell.LockStates = LockStates.Breaking;
            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IAdjcentBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();

                    if (blockItem is IItemEffect effect)
                        effect.PlayMatchEffect();

                    ReleaseGridCell(gridCell);
                }
            }

            gridCell.LockStates = LockStates.None;
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
