using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGameBoardMovementTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        private TimeSpan _gridLockThrottle;
        private IDisposable _gridLockDisposable;

        public bool IsBoardLock { get; private set; }

        public CheckGameBoardMovementTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
            _gridLockThrottle = TimeSpan.FromSeconds(0.5f);
        }

        public void BuildCheckBoard()
        {
            using (var positionPool = ListPool<Vector3Int>.Get(out var activePositions))
            {
                DisposableBuilder builder = Disposable.CreateBuilder();
                activePositions.AddRange(_gridCellManager.GetActivePositions());

                foreach (Vector3Int position in activePositions)
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    gridCell.CheckLockProperty
                            .Subscribe(SetLockValue)
                            .AddTo(ref builder);
                }

                _gridLockDisposable = builder.Build();
            }
        }

        private void SetLockValue(bool isLocked)
        {
            IsBoardLock = isLocked;
        }

        public void Dispose()
        {
            _gridLockDisposable?.Dispose();
        }
    }
}
