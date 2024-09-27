using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class StripedWrappedBoosterTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly IPublisher<CameraShakeMessage> _cameraShakePublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;
        private IDisposable _disposable;

        public StripedWrappedBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            _cts = new();
            _token = _cts.Token;

            _cameraShakePublisher = GlobalMessagePipe.GetPublisher<CameraShakeMessage>();
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            IGridCell actorGridCell = null;
            IGridCell remainGridCell = null;
            IColorBooster actorBooster = null;

            IColorBooster booster1 = gridCell1.BlockItem as IColorBooster;
            IColorBooster booster2 = gridCell2.BlockItem as IColorBooster;

            if (booster1.ColorBoosterType != BoosterType.Wrapped)
            {
                actorBooster = booster1;
                actorGridCell = gridCell1;
                remainGridCell = gridCell2;
            }

            else
            {
                actorBooster = booster2;
                actorGridCell = gridCell2;
                remainGridCell = gridCell1;
            }

            // Remove wrapped booster cell
            actorBooster.IsActivated = true; // Prevent this booster active itself
            _breakGridTask.ReleaseGridCell(remainGridCell);

            actorGridCell.LockStates = LockStates.Breaking;
            Vector3Int checkPosition = actorGridCell.GridPosition;
            BoundsInt activeBounds = _gridCellManager.GetActiveBounds();

            // Lock columns to block all items in these columns falling
            using var columnListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> columnListPositions);
            AddTripleVerticalLine(checkPosition, activeBounds, ref columnListPositions);

            for (int i = 0; i < columnListPositions.Count; i++)
            {
                if (columnListPositions[i] == checkPosition)
                    continue;

                if (columnListPositions[i] == actorGridCell.GridPosition)
                    continue;

                IGridCell gridCell = _gridCellManager.Get(columnListPositions[i]);
                if (gridCell != null && !gridCell.IsLocked)
                    gridCell.LockStates = LockStates.Preparing;
            }

            // Unlock this area to allow booster break these items (3x3 range around center of combo)
            BoundsInt miniCheckBounds = checkPosition.GetBounds2D(1);
            miniCheckBounds.ForEach2D(position =>
            {
                IGridCell gridCell = _gridCellManager.Get(position);
                if (gridCell != null)
                    gridCell.LockStates = LockStates.None;
            });

            actorBooster.ChangeItemSprite(1);
            await actorBooster.PlayBoosterCombo(0, ComboBoosterType.StripedWrapped, true);

            // Break Triple Rows
            using (ListPool<UniTask>.Get(out var breakTasks))
            {
                using var rowListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> rowListPositions);
                AddTripleHorizontalLine(checkPosition, activeBounds, ref rowListPositions);

                for (int i = 0; i < rowListPositions.Count; i++)
                {
                    if (rowListPositions[i] == checkPosition)
                        continue;

                    if (rowListPositions[i] == actorGridCell.GridPosition)
                        continue;

                    breakTasks.Add(_breakGridTask.BreakItem(rowListPositions[i]));
                }

                ShakeCamera();
                SpawnTripleHorizontal(checkPosition);
                UniTask.WhenAll(breakTasks).Forget(); // Use forget to execute the combo independently, fix bug when it hit the colorful booster
                ExpandRange(ref rowListPositions, Vector3Int.up);

                // Lock this area to prevent outside items fall in to
                miniCheckBounds.ForEach2D(position =>
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    if (gridCell != null)
                        gridCell.LockStates = LockStates.Preparing;
                });

                BoundsInt horizontalCheckBounds = BoundsExtension.Encapsulate(rowListPositions);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(horizontalCheckBounds);
            }

            // Wait a moment and enable check grid task to allow other items fall down
            _checkGridTask.CanCheck = true;
            await UniTask.DelayFrame(18, PlayerLoopTiming.FixedUpdate, _token);
            _checkGridTask.CanCheck = false;

            actorBooster.ChangeItemSprite(2);
            await actorBooster.PlayBoosterCombo(0, ComboBoosterType.StripedWrapped, true);

            // Break Triple Columns
            using (ListPool<UniTask>.Get(out var breakTasks))
            {
                for (int i = 0; i < columnListPositions.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(columnListPositions[i]);
                    gridCell.LockStates = LockStates.None;
                    breakTasks.Add(_breakGridTask.BreakItem(columnListPositions[i]));
                }

                ShakeCamera();
                SpawnTripleVertical(checkPosition);
                await UniTask.WhenAll(breakTasks);
                ExpandRange(ref columnListPositions, Vector3Int.right);

                _breakGridTask.ReleaseGridCell(actorGridCell);
                actorGridCell.LockStates = LockStates.None;

                BoundsInt verticalCheckBounds = BoundsExtension.Encapsulate(columnListPositions);
                await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                _checkGridTask.CheckRange(verticalCheckBounds);
            }
        }

        // This function is use to expand check range
        private void ExpandRange(ref List<Vector3Int> positions, Vector3Int expandDirection)
        {
            int positionCount = positions.Count;
            Vector3Int minVertical = positions[0] - expandDirection;
            Vector3Int maxVertical = positions[positionCount - 1] + expandDirection;

            positions.Clear();
            positions.Add(minVertical);
            positions.Add(maxVertical);
        }

        private void AddTripleHorizontalLine(Vector3Int checkPosition, BoundsInt activeBounds, ref List<Vector3Int> positions)
        {
            positions.AddRange(activeBounds.GetRow(checkPosition));
            positions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, -1)));
            positions.AddRange(activeBounds.GetRow(checkPosition + new Vector3Int(0, 1)));
        }

        private void AddTripleVerticalLine(Vector3Int checkPosition, BoundsInt activeBounds, ref List<Vector3Int> positions)
        {
            positions.AddRange(activeBounds.GetColumn(checkPosition));
            positions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(-1, 0)));
            positions.AddRange(activeBounds.GetColumn(checkPosition + new Vector3Int(1, 0)));
        }

        private void SpawnTripleHorizontal(Vector3Int checkPosition)
        {
            Vector3 center = _gridCellManager.ConvertGridToWorldFunction(checkPosition);
            Vector3 down = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.down);
            Vector3 up = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.up);

            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, down);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, center);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Horizontal, up);
        }

        private void SpawnTripleVertical(Vector3Int checkPosition)
        {
            Vector3 center = _gridCellManager.ConvertGridToWorldFunction(checkPosition);
            Vector3 right = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.right);
            Vector3 left = _gridCellManager.ConvertGridToWorldFunction(checkPosition + Vector3Int.left);

            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, left);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, center);
            EffectManager.Instance.SpawnBoosterEffect(ItemType.None, BoosterType.Vertical, right);
        }

        private void ShakeCamera()
        {
            _cameraShakePublisher.Publish(new CameraShakeMessage
            {
                Amplitude = 5f,
                Duration = 0.4f
            });
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
