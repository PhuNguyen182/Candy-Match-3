using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class ColorfulBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly ColorfulFireray _colorfulFireray;

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
            using (var positionListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                IBooster booster = default;
                Vector3 startPosition = boosterCell.WorldPosition;
                colorPositions = FindPositionWithColor(candyColor);
                
                _checkGridTask.IsActive = false;
                if (boosterCell.BlockItem is IBooster colorBooster)
                {
                    booster = colorBooster;
                    await booster.Activate();
                }

                using var fireListPool = ListPool<UniTask>.Get(out List<UniTask> fireTasks);
                using var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    fireTasks.Add(Fireray(colorPositions[i], startPosition, i * 0.02f));
                }

                await UniTask.WhenAll(fireTasks);
                float delay = colorPositions.Count * 0.02f + 0.25f;
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _token);

                booster.Explode();
                _breakGridTask.ReleaseGridCell(boosterCell);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    breakTasks.Add(_breakGridTask.Break(colorPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);
                _checkedCandyColors.Remove(candyColor);

                if (_checkedCandyColors.Count <= 0)
                    _checkGridTask.IsActive = true;
            }
        }

        public async UniTask Activate(Vector3Int checkPosition)
        {
            CandyColor checkColor = CandyColor.None;
            IGridCell gridCell = _gridCellManager.Get(checkPosition);

            _checkGridTask.IsActive = false;
            using (var positionListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                IBooster booster = default;
                Vector3 startPosition = gridCell.WorldPosition;
                colorPositions = FindMostFrequentColor();

                if(colorPositions.Count > 0)
                {
                    IGridCell colorCell = _gridCellManager.Get(colorPositions[0]);
                    checkColor = colorCell.CandyColor;
                }

                if (gridCell.BlockItem is IBooster colorBooster)
                {
                    booster = colorBooster;
                    await booster.Activate();
                }

                using var fireListPool = ListPool<UniTask>.Get(out List<UniTask> fireTasks);
                using var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    fireTasks.Add(Fireray(colorPositions[i], startPosition, i * 0.02f));
                }

                await UniTask.WhenAll(fireTasks);
                float delay = colorPositions.Count * 0.02f + 0.25f;
                await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: _token);

                booster.Explode();
                _breakGridTask.ReleaseGridCell(gridCell);

                for (int i = 0; i < colorPositions.Count; i++)
                {
                    breakTasks.Add(_breakGridTask.Break(colorPositions[i]));
                }

                await UniTask.WhenAll(breakTasks);
                _checkedCandyColors.Remove(checkColor);

                if(_checkedCandyColors.Count <= 0)
                    _checkGridTask.IsActive = true;
            }
        }

        public List<Vector3Int> FindPositionWithColor(CandyColor color)
        {
            _checkedCandyColors.Add(color);
            List<Vector3Int> colorPositions = new();

            using(var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_gridCellManager.Iterator());

                using (var foundListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> foundPositions))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(positions[i]);

                        if (gridCell == null)
                            continue;

                        if (gridCell.CandyColor != color)
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
            using (var hashedItemPool = DictionaryPool<CandyColor, List<Vector3Int>>.Get(out var itemCollection))
            {
                using (var positionListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
                {
                    positions.AddRange(_gridCellManager.Iterator());

                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(positions[i]);

                        if (gridCell == null)
                            continue;

                        // Not a color item
                        if (gridCell.CandyColor == CandyColor.None)
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

        private async UniTask Fireray(Vector3Int targetPosition, Vector3 position, float delay)
        {
            IGridCell targetGridCell = _gridCellManager.Get(targetPosition);
            ColorfulFireray fireray = SimplePool.Spawn(_colorfulFireray, EffectContainer.Transform
                                                       , Vector3.zero, Quaternion.identity);
            await fireray.Fire(targetGridCell, position, delay);
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
