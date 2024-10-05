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
using UnityEngine.Pool;
using UnityEngine.UIElements;

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
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
            };

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask BreakItem(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell == null)
                return;

            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                bool isLockedState = gridCell.GridStateful.IsLocked;

                if (stateBreakable.Break())
                {
                    _checkGridTask.CheckMatchAtPosition(position);
                    gridCell.SetGridStateful(new AvailableState());
                }

                if (isLockedState)
                {
                    _checkGridTask.CheckAroundPosition(position, 1);
                    return;
                }
            }

            if (gridCell.HasItem)
            {
                if (gridCell.IsLocked)
                    return;

                gridCell.LockStates = LockStates.Breaking;
                IBlockItem blockItem = gridCell.BlockItem;

                if (blockItem is IBooster booster)
                {
                    if (booster.IsNewCreated)
                    {
                        gridCell.LockStates = LockStates.None;
                        return;
                    }

                    await _activateBoosterTask.ActivateBooster(gridCell, true, false, false);
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                if (blockItem is IBreakable breakable)
                {
                    if (breakable.Break())
                    {
                        blockItem.ItemBlast().Forget();

                        if (blockItem is IItemEffect effect)
                            effect.PlayBreakEffect();

                        ReleaseGridCell(gridCell);
                    }
                }
            }

            gridCell.LockStates = LockStates.None;
        }

        public async UniTask Break(Vector3Int position, bool breakAdjacent = false)
        {
            IGridCell gridCell = _gridCellManager.Get(position);
            await Break(gridCell, breakAdjacent);
        }

        public async UniTask Break(IGridCell gridCell, bool breakAdjacent)
        {
            if (gridCell == null)
                return;

            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                Vector3Int position = gridCell.GridPosition;
                bool isLockedState = gridCell.GridStateful.IsLocked;

                if (stateBreakable.Break())
                {
                    _checkGridTask.CheckMatchAtPosition(position);
                    gridCell.SetGridStateful(new AvailableState());
                }

                if (isLockedState)
                {
                    _checkGridTask.CheckAroundPosition(position, 1);
                    return;
                }
            }

            if (!gridCell.HasItem)
                return;

            if (gridCell.IsLocked)
                return;

            gridCell.LockStates = LockStates.Breaking;
            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                if (booster.IsNewCreated)
                {
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                await _activateBoosterTask.ActivateBooster(gridCell, true, false, false);
                gridCell.LockStates = LockStates.None;
                return;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    await blockItem.ItemBlast();

                    if (blockItem is IItemEffect effect)
                        effect.PlayBreakEffect();

                    ReleaseGridCell(gridCell);
                }

                if (breakAdjacent)
                {
                    using (ListPool<UniTask>.Get(out List<UniTask> breakTasks))
                    {
                        for (int i = 0; i < _adjacentStepCheck.Count; i++)
                        {
                            Vector3Int checkPos = gridCell.GridPosition + _adjacentStepCheck[i];
                            IGridCell cell = _gridCellManager.Get(checkPos);
                            breakTasks.Add(BreakAdjacent(cell));
                        }

                        await UniTask.WhenAll(breakTasks);
                    }
                }

                gridCell.LockStates = LockStates.None;
                _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);
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
                {
                    _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);
                    return;
                }
            }

            if (gridCell.IsLocked)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if (gridCell.HasItem)
            {
                if (blockItem is IBooster booster)
                {
                    if (booster.IsNewCreated)
                    {
                        gridCell.LockStates = LockStates.None;
                        return;
                    }

                    await _activateBoosterTask.ActivateBooster(gridCell, true, true, true, onActive);
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

            TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);

            if (gridCell.BlockItem is IItemEffect effect)
                effect.PlayStartEffect();

            gridCell.LockStates = LockStates.None;
        }

        public async UniTask BreakMatchItem(IGridCell gridCell, Vector3 matchPivot, MatchType matchType, Action<BoundsInt> onActive = null)
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
                {
                    _checkGridTask.CheckAroundPosition(position, 1);
                    return;
                }
            }

            if (gridCell.IsLocked)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            gridCell.LockStates = LockStates.Matching;

            if (blockItem is IBooster booster)
            {
                if (booster.IsNewCreated)
                {
                    gridCell.LockStates = LockStates.None;
                    return;
                }

                await _activateBoosterTask.ActivateBooster(gridCell, true, true, false, onActive);
                
                gridCell.LockStates = LockStates.None;
                return;
            }

            if (blockItem is IBreakable breakable)
            {
                if (breakable.Break())
                {
                    if (matchType == MatchType.Match3)
                    {
                        await blockItem.ItemBlast();

                        if (blockItem is IItemEffect effect)
                            effect.PlayMatchEffect();
                    }

                    else
                    {
                        if(blockItem is IMatchAnimation matchAnimation)
                        {
                            float duration = Match3Constants.ItemMatchDelay;
                            await matchAnimation.MatchTo(matchPivot, duration);
                        }
                    }

                    ReleaseGridCell(gridCell);
                }
            }

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

            IBlockItem blockItem = gridCell.BlockItem;
            if (blockItem is IAdjcentBreakable breakable)
            {
                gridCell.LockStates = LockStates.Breaking;

                if (breakable.Break())
                {
                    await blockItem.ItemBlast();

                    if (blockItem is IItemEffect effect)
                        effect.PlayBreakEffect();

                    ReleaseGridCell(gridCell);
                }

                _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);
                gridCell.LockStates = LockStates.None;
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
