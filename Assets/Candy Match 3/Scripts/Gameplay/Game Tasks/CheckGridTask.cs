using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using GlobalScripts.UpdateHandlerPattern;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGridTask : IDisposable, IFixedUpdateHandler
    {
        private readonly MatchRegionTask _matchRegionTask;
        private readonly GridCellManager _gridCellManager;
        private readonly MoveItemTask _moveItemTask;
        private readonly SpawnItemTask _spawnItemTask;

        private List<Vector3Int> _positionsToCheck;
        private HashSet<Vector3Int> _checkPositions;

        private bool _checkMatch = false;
        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public bool CanCheck { get; set; }
        public bool IsActive { get; set; }

        public CheckGridTask(GridCellManager gridCellManager, MoveItemTask moveItemTask
            , SpawnItemTask spawnItemTask, MatchRegionTask matchRegionTask)
        {
            _moveItemTask = moveItemTask;
            _gridCellManager = gridCellManager;
            _spawnItemTask = spawnItemTask;
            _matchRegionTask = matchRegionTask;

            _checkPositions = new();
            _positionsToCheck = new();

            _cts = new();
            _token = _cts.Token;

            CanCheck = true;
            IsActive = true;

            UpdateHandlerManager.Instance.AddFixedUpdateBehaviour(this);
        }

        public void OnFixedUpdate()
        {
            if (!CanCheck)
                return;

            if (_checkPositions.Count > 0)
            {
                _positionsToCheck.Clear();
                _positionsToCheck.AddRange(_checkPositions);
                _checkPositions.Clear();

                for (int i = 0; i < _positionsToCheck.Count; i++)
                {
                    IGridCell checkCell = _gridCellManager.Get(_positionsToCheck[i]);

                    if (_spawnItemTask.CheckSpawnable(checkCell))
                    {
                        _spawnItemTask.Spawn(checkCell.GridPosition).Forget();
                    }

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

        public void CheckMatchAtPosition(Vector3Int position)
        {
            _matchRegionTask.CheckMatchRegion(position);
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

        public void CheckAt(Vector3Int position)
        {
            _checkPositions.Add(position);
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
            _positionsToCheck.Clear();
            _checkPositions.Clear();

            UpdateHandlerManager.Instance.RemoveFixedUpdateBehaviour(this);
        }
    }
}
