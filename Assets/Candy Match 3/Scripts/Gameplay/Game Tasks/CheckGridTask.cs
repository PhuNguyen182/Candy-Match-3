using R3;
using System;
using System.Threading;
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
        private readonly GridCellManager _gridCellManager;
        private readonly MoveItemTask _moveItemTask;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly SpawnItemTask _spawnItemTask;

        private bool _anyItemMove;
        private List<Vector3Int> _positionsToCheck;
        private HashSet<Vector3Int> _checkPositions;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        private IDisposable _disposable;

        public CheckGridTask(GridCellManager gridCellManager, MoveItemTask moveItemTask, SpawnItemTask spawnItemTask, MatchItemsTask matchItemsTask)
        {
            _moveItemTask = moveItemTask;
            _gridCellManager = gridCellManager;
            _spawnItemTask = spawnItemTask;
            _matchItemsTask = matchItemsTask;

            _positionsToCheck = new();
            _checkPositions = new();

            _cts = new();
            _token = _cts.Token;

            DisposableBuilder builder = Disposable.CreateBuilder();
            
            Observable.EveryUpdate(UnityFrameProvider.Update)
                      .Subscribe(_ => Update()).AddTo(ref builder);
            
            _disposable = builder.Build();
        }

        private void Update()
        {
            if (_checkPositions.Count > 0)
            {
                _positionsToCheck.Clear();
                _positionsToCheck.AddRange(_checkPositions);
                _checkPositions.Clear();

                _anyItemMove = false;
                for (int i = 0; i < _positionsToCheck.Count; i++)
                {
                    IGridCell checkCell = _gridCellManager.Get(_positionsToCheck[i]);

                    if (_spawnItemTask.CheckSpawnable(checkCell))
                    {
                        _spawnItemTask.Spawn(checkCell.GridPosition).Forget();
                    }

                    if (_moveItemTask.CheckMoveable(checkCell))
                    {
                        _anyItemMove = true;
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

        public void CheckMatchAtPosition(Vector3Int position)
        {
            if(_matchItemsTask.CheckMatchAt(position))
            {
                _matchItemsTask.Match(position).Forget();
            }
        }

        public void CheckCross(Vector3Int position, bool checkSelf = true)
        {
            if(checkSelf)
                _checkPositions.Add(position);
            
            _checkPositions.Add(position + Vector3Int.left);
            _checkPositions.Add(position + Vector3Int.right);
            _checkPositions.Add(position + Vector3Int.down);
            _checkPositions.Add(position + Vector3Int.up);
        }

        public void CheckRange(BoundsInt boundsRange)
        {
            AddRangeToCheck(boundsRange);
        }

        public void CheckAroundPosition(Vector3Int position, int range)
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
            _cts.Dispose();
            _disposable.Dispose();

            _positionsToCheck.Clear();
            _checkPositions.Clear();
        }
    }
}
