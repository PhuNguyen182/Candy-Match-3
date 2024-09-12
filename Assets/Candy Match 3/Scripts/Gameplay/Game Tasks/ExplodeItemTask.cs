using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class ExplodeItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public ExplodeItemTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask Blast(Vector3Int pivot, Vector3Int size)
        {
            BoundsInt bounds = pivot.GetBounds2D(size);
            await Blast(pivot, bounds);
        }

        public async UniTask Blast(Vector3Int pivot, int range)
        {
            BoundsInt bounds = pivot.GetBounds2D(range);
            await Blast(pivot, bounds);
        }

        public async UniTask Blast(Vector3Int pivot, BoundsInt range)
        {
            using(ListPool<Vector3Int>.Get(out List<Vector3Int> boundsEdge))
            {
                boundsEdge.AddRange(range.Iterator2D());
                IGridCell centerCell = _gridCellManager.Get(pivot);
                Vector3 centerPosition = centerCell.WorldPosition;

                IGridCell gridCell = null;
                using (ListPool<UniTask>.Get(out List<UniTask> explodeTasks))
                {
                    for (int i = 0; i < boundsEdge.Count; i++)
                    {
                        if (boundsEdge[i] == pivot)
                            continue;

                        gridCell = _gridCellManager.Get(boundsEdge[i]);

                        if (gridCell == null)
                            continue;

                        if (!gridCell.HasItem)
                            continue;

                        if (gridCell.BlockItem is IItemAnimation animation)
                        {
                            Vector3 blockPosition = gridCell.WorldPosition;
                            Vector3 direction = (centerPosition - gridCell.WorldPosition).normalized;

                            float distance = GetDistance(pivot, gridCell.GridPosition);
                            float bounce = Match3Constants.ExplodeAmplitude * Mathf.Log(distance, Match3Constants.ExplosionPower);
                            explodeTasks.Add(animation.BounceInDirection(blockPosition + direction * bounce));
                        }
                    }

                    await UniTask.WhenAll(explodeTasks);
                    await UniTask.NextFrame(_token);
                }
            }
        }

        private float GetDistance(Vector3Int center, Vector3Int toPosition)
        {
            Vector3Int offset = toPosition - center;
            float distance = offset.magnitude;
            return distance;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
