using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Messages;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class DoubleWrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly BreakGridTask _breakGridTask;
        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private CheckGridTask _checkGridTask;

        public DoubleWrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ExplodeItemTask explodeItemTask)
        {
            _gridCellManager = gridCellManager;
            _explodeItemTask = explodeItemTask;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;

            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                Vector3Int direction = gridCell2.GridPosition - gridCell1.GridPosition;
                BoundsInt attackRange = GetSizeFromDirection(gridCell1.GridPosition, gridCell2.GridPosition);
                BoundsInt affectRange = GetAffectRangeFromDirection(gridCell1.GridPosition, gridCell2.GridPosition);

                int boosterComboDirection1 = GetDirectionFromSwap(direction);
                int boosterComboDirection2 = GetDirectionFromSwap(-direction);

                IBlockItem firstItem = gridCell2.BlockItem;
                IBlockItem secondItem = gridCell1.BlockItem;

                if(firstItem is IColorBooster firstBooster && secondItem is IColorBooster secondBooster)
                {
                    UniTask boosterTask1 = firstBooster.PlayBoosterCombo(boosterComboDirection1, ComboBoosterType.DoubleWrapped, true);
                    UniTask boosterTask2 = secondBooster.PlayBoosterCombo(boosterComboDirection2, ComboBoosterType.DoubleWrapped, false);
                    await UniTask.WhenAll(boosterTask1, boosterTask2);
                }

                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);

                Vector3Int checkPosition = gridCell2.GridPosition;
                positions.AddRange(attackRange.Iterator2D());
                _explodeItemTask.Blast(checkPosition, affectRange).Forget();

                using (ListPool<UniTask>.Get(out List<UniTask> breakTasks))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        breakTasks.Add(_breakGridTask.BreakItem(positions[i]));
                    }

                    ShakeCamera();
                    await UniTask.WhenAll(breakTasks);

                    await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                }

                int count = positions.Count;
                Vector3Int min = positions[0] + new Vector3Int(-1, -1);
                Vector3Int max = positions[count - 1] + new Vector3Int(1, 1);

                positions.Clear();
                positions.Add(min);
                positions.Add(max);

                BoundsInt checkRange = BoundsExtension.Encapsulate(positions);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(checkRange);
            }
        }

        private BoundsInt GetSizeFromDirection(Vector3Int from, Vector3Int to)
        {
            Vector3Int direction = to - from;

            if (direction == Vector3Int.right || direction == Vector3Int.up)
            {
                return new BoundsInt
                {
                    min = from + new Vector3Int(-2, -2),
                    max = to + new Vector3Int(3, 3)
                };
            }

            if (direction == Vector3Int.left || direction == Vector3Int.down)
            {
                return new BoundsInt
                {
                    min = to + new Vector3Int(-2, -2),
                    max = from + new Vector3Int(3, 3)
                };
            }

            return new BoundsInt();
        }

        private BoundsInt GetAffectRangeFromDirection(Vector3Int from, Vector3Int to)
        {
            Vector3Int direction = to - from;

            if (direction == Vector3Int.right || direction == Vector3Int.up)
            {
                return new BoundsInt
                {
                    min = from + new Vector3Int(-4, -4),
                    max = to + new Vector3Int(5, 5)
                };
            }

            if (direction == Vector3Int.left || direction == Vector3Int.down)
            {
                return new BoundsInt
                {
                    min = to + new Vector3Int(-4, -4),
                    max = from + new Vector3Int(5, 5)
                };
            }

            return new BoundsInt();
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        private void ShakeCamera()
        {
            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 3f,
                Duration = 0.35f
            });
        }

        private int GetDirectionFromSwap(Vector3Int direction)
        {
            int dir = 0;

            if (direction == Vector3Int.up)
                dir = 1;
            else if (direction == Vector3Int.down)
                dir = 2;
            else if (direction == Vector3Int.left)
                dir = 3;
            else if (direction == Vector3Int.right)
                dir = 4;

            return dir;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
