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

        public async UniTask Blast(Vector3Int center, BoundsInt range)
        {
            using(var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> boundsEdge))
            {
                boundsEdge.AddRange(range.Iterator2D());
                IGridCell centerCell = _gridCellManager.Get(center);
                Vector3 centerPosition = centerCell.WorldPosition;

                IGridCell gridCell = null;
                using (var explodeListPool = ListPool<UniTask>.Get(out List<UniTask> explodeTasks))
                {
                    for (int i = 0; i < boundsEdge.Count; i++)
                    {
                        gridCell = _gridCellManager.Get(boundsEdge[i]);

                        if (gridCell == null)
                            continue;

                        if (!gridCell.HasItem)
                            continue;

                        Vector3 direction = (gridCell.WorldPosition - centerPosition).normalized;

                        if (gridCell.BlockItem is IItemAnimation animation)
                        {
                            direction = direction * Match3Constants.ExplodeAmplitude;
                            explodeTasks.Add(animation.BounceInDirection(direction));
                        }
                    }

                    await UniTask.WhenAll(explodeTasks);
                    await UniTask.NextFrame(_token);
                }
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
