using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class ColorfulBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public ColorfulBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask ActivateWithColor(CandyColor candyColor)
        {
            using (var positionListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                colorPositions = FindPositionWithColor(candyColor);

                using (var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
                {
                    for (int i = 0; i < colorPositions.Count; i++)
                    {
                        breakTasks.Add(_breakGridTask.Break(colorPositions[i]));
                    }

                    await UniTask.DelayFrame(6, PlayerLoopTiming.Update, _token);
                    await UniTask.WhenAll(breakTasks);
                }
            }
        }

        public async UniTask Activate()
        {
            using(var positionListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> colorPositions))
            {
                colorPositions = FindMostFrequentColor();

                using (var breakListPool = ListPool<UniTask>.Get(out List<UniTask> breakTasks))
                {
                    for (int i = 0; i < colorPositions.Count; i++)
                    {
                        breakTasks.Add(_breakGridTask.Break(colorPositions[i]));
                    }

                    await UniTask.DelayFrame(6, PlayerLoopTiming.Update, _token);
                    await UniTask.WhenAll(breakTasks);
                }
            }
        }

        public List<Vector3Int> FindPositionWithColor(CandyColor color)
        {
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

                    foreach (var foundItems in itemCollection)
                    {
                        if (foundItems.Value.Count > maxCount)
                        {
                            maxCount = foundItems.Value.Count;
                            foundPositions = foundItems.Value;
                        }
                    }

                    return foundPositions;
                }
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
