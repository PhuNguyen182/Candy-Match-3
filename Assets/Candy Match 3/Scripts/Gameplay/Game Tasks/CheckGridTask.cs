using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGridTask : IDisposable
    {
        private readonly MoveItemTask _moveItemTask;
        private readonly GridCellManager _gridCellManager;

        private List<Vector3Int> _positionsToCheck;
        private HashSet<Vector3Int> _checkPositions;

        private IDisposable _disposable;

        public CheckGridTask(GridCellManager gridCellManager, MoveItemTask moveItemTask)
        {
            _moveItemTask = moveItemTask;
            _gridCellManager = gridCellManager;

            _positionsToCheck = new();
            _checkPositions = new();

            DisposableBuilder builder = Disposable.CreateBuilder();
            
            Observable.EveryUpdate(UnityFrameProvider.FixedUpdate)
                      .Subscribe(_ => Update()).AddTo(ref builder);
            
            _disposable = builder.Build();
        }

        private void Update()
        {
            if(_checkPositions.Count > 0)
            {
                _positionsToCheck.Clear();
                _positionsToCheck.AddRange(_checkPositions);
                _checkPositions.Clear();

                for (int i = 0; i < _positionsToCheck.Count; i++)
                {
                    IGridCell checkCell = _gridCellManager.Get(_positionsToCheck[i]);

                    if (_moveItemTask.CheckMoveable(checkCell))
                    {
                        _moveItemTask.MoveItem(checkCell).Forget();
                    }
                }
            }
        }

        private void AddRangeToCheck(BoundsInt bounds)
        {
            foreach (Vector3Int position in bounds.Iterator2D())
            {
                _checkPositions.Add(position);
            }
        }

        public void CheckAt(Vector3Int position, int range)
        {
            BoundsInt checkRange = position.GetBounds2D(range);
            AddRangeToCheck(checkRange);
        }

        public void CheckInDirection(Vector3Int position, Vector3Int direction)
        {
            _checkPositions.Add(position + direction);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
