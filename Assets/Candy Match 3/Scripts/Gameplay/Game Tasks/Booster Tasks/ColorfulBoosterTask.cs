using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class ColorfulBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly ColorfulFireray _colorfulFireray;

        private int _activateCount = 0;
        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;
        private HashSet<CandyColor> _checkedCandyColors;

        public ColorfulBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ColorfulFireray colorfulFireray)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _colorfulFireray = colorfulFireray;

            _cts = new();
            _token = _cts.Token;
            _checkedCandyColors = new();
        }

        public async UniTask ActivateWithColor(IGridCell boosterCell, CandyColor candyColor)
        {
            _checkGridTask.CanCheck = false;
            _activateCount = _activateCount + 1;

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                IBooster booster = boosterCell.BlockItem as IBooster;
                Vector3 startPosition = boosterCell.WorldPosition;
                colorPositions = FindPositionWithColor(candyColor);

                booster.IsActivated = true;
                await booster.Activate();

                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                using var fireListPool = ListPool<UniTask>.Get(out List<UniTask> fireTasks);
                using var breakTaskPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);
                PlayEffect();

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    fireTasks.Add(FireItemCatchRay(i, colorPositions[i], startPosition, i * 0.02f));
                }

                await UniTask.WhenAll(fireTasks);
                TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay * 1.5f);
                await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    breakTasks.Add(_breakGridTask.Break(colorPositions[i], true));
                }

                await UniTask.WhenAll(breakTasks);

                booster.Explode();
                _breakGridTask.ReleaseGridCell(boosterCell);
                RemoveColor(candyColor);

                _activateCount = _activateCount - 1;
                boosterCell.LockStates = LockStates.None;
                _checkGridTask.CheckAroundPosition(boosterCell.GridPosition, 1);

                if (_checkedCandyColors.Count <= 0 && _activateCount <= 0)
                    _checkGridTask.CanCheck = true;
            }
        }

        public async UniTask Activate(Vector3Int checkPosition)
        {
            _activateCount = _activateCount + 1;
            CandyColor checkColor = CandyColor.None;

            IGridCell gridCell = _gridCellManager.Get(checkPosition);
            gridCell.LockStates = LockStates.Preparing;
            _checkGridTask.CanCheck = false;

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                IBooster booster = gridCell.BlockItem as IBooster;
                Vector3 startPosition = gridCell.WorldPosition;
                colorPositions = FindMostFrequentColor();

                if (colorPositions.Count > 0)
                {
                    IGridCell colorCell = _gridCellManager.Get(colorPositions[0]);
                    checkColor = colorCell.CandyColor;
                }

                booster.IsActivated = true;
                await booster.Activate();

                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                using var fireListPool = ListPool<UniTask>.Get(out List<UniTask> fireTasks);
                using var breakTaskPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);
                PlayEffect();

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    fireTasks.Add(FireItemCatchRay(i, colorPositions[i], startPosition, i * 0.02f));
                }

                await UniTask.WhenAll(fireTasks);
                TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay * 1.5f);
                await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    breakTasks.Add(_breakGridTask.Break(colorPositions[i], true));
                }

                await UniTask.WhenAll(breakTasks);

                booster.Explode();
                _breakGridTask.ReleaseGridCell(gridCell);
                RemoveColor(checkColor);

                _activateCount = _activateCount - 1;
                gridCell.LockStates = LockStates.None;
                _checkGridTask.CheckAroundPosition(gridCell.GridPosition, 1);

                if (_checkedCandyColors.Count <= 0 && _activateCount <= 0)
                    _checkGridTask.CanCheck = true;
            }
        }

        public List<Vector3Int> FindPositionWithColor(CandyColor color)
        {
            _checkedCandyColors.Add(color);
            List<Vector3Int> colorPositions = new();

            using(ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_gridCellManager.GetActivePositions());

                using (ListPool<Vector3Int>.Get(out List<Vector3Int> foundPositions))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(positions[i]);

                        if (gridCell == null || !gridCell.HasItem)
                            continue;

                        if (gridCell.CandyColor != color || gridCell.IsLocked)
                            continue;

                        if (!gridCell.BlockItem.IsMatchable || gridCell.BlockItem.IsLocking)
                            continue;

                        foundPositions.Add(positions[i]);
                    }

                    colorPositions.AddRange(foundPositions);
                    return colorPositions;
                }
            }
        }

        public List<Vector3Int> FindMostFrequentColor()
        {
            using (DictionaryPool<CandyColor, List<Vector3Int>>.Get(out var itemCollection))
            {
                using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
                {
                    positions.AddRange(_gridCellManager.GetActivePositions());

                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(positions[i]);

                        if (gridCell == null || !gridCell.HasItem)
                            continue;

                        // Not a color item
                        if (gridCell.CandyColor == CandyColor.None || gridCell.IsLocked)
                            continue;

                        if (!gridCell.BlockItem.IsMatchable || gridCell.BlockItem.IsLocking)
                            continue;

                        // Prevent duplicate color detection
                        if (_checkedCandyColors.Contains(gridCell.CandyColor))
                            continue;

                        if (itemCollection.ContainsKey(gridCell.BlockItem.CandyColor))
                        {
                            List<Vector3Int> colorPositions = itemCollection[gridCell.BlockItem.CandyColor];

                            colorPositions.Add(positions[i]);
                            itemCollection[gridCell.BlockItem.CandyColor] = colorPositions;
                        }

                        else
                        {
                            List<Vector3Int> colorPositions = new() { positions[i] };
                            itemCollection.Add(gridCell.BlockItem.CandyColor, colorPositions);
                        }
                    }

                    int maxCount = -1;
                    List<Vector3Int> foundPositions = new();
                    CandyColor checkColor = CandyColor.None;

                    foreach (var foundItems in itemCollection)
                    {
                        if (foundItems.Value.Count > maxCount)
                        {
                            maxCount = foundItems.Value.Count;
                            foundPositions = foundItems.Value;
                            checkColor = foundItems.Key;
                        }
                    }

                    if (checkColor != CandyColor.None)
                        _checkedCandyColors.Add(checkColor);

                    return foundPositions;
                }
            }
        }

        private async UniTask FireItemCatchRay(int index, Vector3Int targetPosition, Vector3 position, float delay)
        {
            IGridCell targetGridCell = _gridCellManager.Get(targetPosition);

            if (targetGridCell != null && targetGridCell.HasItem)
            {
                ColorfulFireray fireray = SimplePool.Spawn(_colorfulFireray, EffectContainer.Transform
                                                           , Vector3.zero, Quaternion.identity);
                fireray.SetPhaseStep(index);
                if (targetGridCell.GridStateful.CanContainItem)
                {
                    targetGridCell.BlockItem.IsLocking = true;
                    targetGridCell.BlockItem.SetMatchable(false);
                }

                await fireray.Fire(targetGridCell, position, delay);
            }
        }

        private void PlayEffect()
        {
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ColorfulCastItems);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ColorfulFirerayCluster);
        }

        public void RemoveColor(CandyColor candyColor)
        {
            _checkedCandyColors.Remove(candyColor);
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _cts.Dispose();
            _checkedCandyColors.Clear();
        }
    }
}
