using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using GlobalScripts.Extensions;
using GlobalScripts.Comparers;
using UnityEngine.Pool;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MatchItemsTask : IDisposable
    {
        private readonly MatchRule _matchRule;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private CheckGridTask _checkGridTask;
        private Vector3IntComparer _vector3IntComparer;
        private List<Vector3Int> _adjacentSteps;
        private IDisposable _disposable;

        public MatchItemsTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _matchRule = new(_gridCellManager);
            _breakGridTask = breakGridTask;

            _adjacentSteps = new()
            {
                new(1, 0),
                new(0, 1),
                new(-1, 0),
                new(0, -1)
            };

            _vector3IntComparer = new();
            DisposableBuilder builder = Disposable.CreateBuilder();
            _matchRule.AddTo(ref builder);
            _disposable = builder.Build();
        }

        public bool CheckMatch(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell.HasItem && gridCell.BlockItem.IsMatchable)
            {
                bool isMatch = _matchRule.CheckMatch(position, out MatchResult matchResult);

                if (isMatch)
                    _breakGridTask.BreakMatch(matchResult).Forget();

                return isMatch;
            }

            return false;
        }

        public async UniTask Match(Vector3Int position)
        {
            MatchResult matchResult;
            bool isMatch = _matchRule.CheckMatch(position, out matchResult);

            if (isMatch)
            {
                await ProcessMatch(matchResult);
            }
        }

        private async UniTask ProcessMatch(MatchResult matchResult)
        {
            MatchType matchType = matchResult.MatchType;
            CandyColor candyColor = matchResult.CandyColor;

            using (PooledObject<List<UniTask>> matchListPool = ListPool<UniTask>.Get(out List<UniTask> matchTasks))
            {
                using var boundListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> boundPositions);
                using var matchAdjacent = HashSetPool<Vector3Int>.Get(out HashSet<Vector3Int> adjacentPositions);

                for (int i = 0; i < matchResult.MatchSequence.Count; i++)
                {
                    Vector3Int position = matchResult.MatchSequence[i];
                    
                    boundPositions.Add(position);
                    IGridCell gridCell = _gridCellManager.Get(position);

                    if (matchType != MatchType.Match3 && gridCell.GridPosition == matchResult.Position)
                    {
                        matchTasks.Add(_breakGridTask.SpawnBooster(gridCell, matchType, candyColor));
                    }

                    else
                    {
                        matchTasks.Add(_breakGridTask.BreakMatchItem(gridCell, matchResult.MatchSequence.Count, matchType));
                    }

                    for (int j = 0; j < _adjacentSteps.Count; j++)
                    {
                        adjacentPositions.Add(position + _adjacentSteps[j]);
                    }
                }

                int count = boundPositions.Count;
                boundPositions.Sort(_vector3IntComparer);
                
                Vector3Int min = boundPositions[0] + new Vector3Int(-1, -1);
                Vector3Int max = boundPositions[count - 1] + new Vector3Int(1, 1);
                
                boundPositions.Add(min);
                boundPositions.Add(max);

                List<Vector3Int> adjacentPositionList = adjacentPositions.ToList();

                for (int i = 0; i < adjacentPositionList.Count; i++)
                {
                    Vector3Int position = adjacentPositionList[i];
                    IGridCell gridCell = _gridCellManager.Get(position);
                    matchTasks.Add(_breakGridTask.BreakAdjacent(gridCell));
                }

                adjacentPositionList.Clear();
                await UniTask.WhenAll(matchTasks);

                BoundsInt checkMatchBounds = BoundsExtension.Encapsulate(boundPositions);
                _checkGridTask.CheckRange(checkMatchBounds);
            }
        }

        public bool CheckMatchAt(Vector3Int checkPosition)
        {
            IGridCell gridCell = _gridCellManager.Get(checkPosition);
            return gridCell.HasItem && gridCell.BlockItem.IsMatchable;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _adjacentSteps.Clear();
            _disposable.Dispose();
        }
    }
}
