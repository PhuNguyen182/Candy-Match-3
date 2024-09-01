using R3;
using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Strategies;
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

        private CancellationToken _token;
        private CancellationTokenSource _cts;

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

            _cts = new();
            _token = _cts.Token;

            _vector3IntComparer = new();
            DisposableBuilder builder = Disposable.CreateBuilder();
            _matchRule.AddTo(ref builder);
            _disposable = builder.Build();
        }

        public bool CheckMatchInSwap(Vector3Int position)
        {
            if(CheckMatchAt(position))
            {
                MatchResult matchResult;
                bool isMatch = IsMatchable(position, out matchResult);
                
                if(isMatch)
                    ProcessMatch(matchResult).Forget();

                return isMatch;
            }

            return false;
        }

        public async UniTask<bool> Match(Vector3Int position)
        {
            MatchResult matchResult;
            bool isMatch = IsMatchable(position, out matchResult);

            if (isMatch)
            {
                await ProcessMatch(matchResult);
            }

            return isMatch;
        }

        private async UniTask ProcessMatch(MatchResult matchResult)
        {
            bool hasBooster = false;
            MatchType matchType = matchResult.MatchType;
            CandyColor candyColor = matchResult.CandyColor;

            using (var matchListPool = ListPool<UniTask>.Get(out List<UniTask> matchTasks))
            {
                using (var boundListPool = ListPool<Vector3Int>.Get(out List<Vector3Int> boundPositions))
                {
                    using var matchAdjacent = HashSetPool<Vector3Int>.Get(out HashSet<Vector3Int> adjacentPositions);

                    for (int i = 0; i < matchResult.MatchSequence.Count; i++)
                    {
                        Vector3Int position = matchResult.MatchSequence[i];

                        boundPositions.Add(position);
                        IGridCell gridCell = _gridCellManager.Get(position);
                        IBlockItem blockItem = gridCell.BlockItem;
                        gridCell.IsMatching = true;

                        if (blockItem is IColorBooster)
                            hasBooster = true;

                        if (matchType != MatchType.Match3 && gridCell.GridPosition == matchResult.Position)
                            matchTasks.Add(_breakGridTask.AddBooster(gridCell, matchType, candyColor));

                        else
                            matchTasks.Add(_breakGridTask.BreakMatchItem(gridCell, matchResult.MatchSequence.Count));

                        for (int j = 0; j < _adjacentSteps.Count; j++)
                            adjacentPositions.Add(position + _adjacentSteps[j]);
                    }

                    foreach (Vector3Int adjacentPosition in adjacentPositions)
                    {
                        IGridCell gridCell = _gridCellManager.Get(adjacentPosition);
                        matchTasks.Add(_breakGridTask.BreakAdjacent(gridCell));
                    }

                    int count = boundPositions.Count;
                    boundPositions.Sort(_vector3IntComparer);

                    Vector3Int min = !hasBooster ? boundPositions[0] + new Vector3Int(-1, -1) : boundPositions[0] + new Vector3Int(-2, -2);
                    Vector3Int max = !hasBooster ? boundPositions[count - 1] + new Vector3Int(1, 1) : boundPositions[count - 1] + new Vector3Int(2, 2);

                    boundPositions.Add(min);
                    boundPositions.Add(max);

                    await UniTask.WhenAll(matchTasks);
                    BoundsInt checkMatchBounds = BoundsExtension.Encapsulate(boundPositions);
                    await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);
                    _checkGridTask.CheckRange(checkMatchBounds);
                }
            }
        }

        public bool CheckMatchAt(Vector3Int checkPosition)
        {
            IGridCell gridCell = _gridCellManager.Get(checkPosition);
            
            if (!gridCell.HasItem)
                return false;

            if (!gridCell.BlockItem.IsMatchable || gridCell.IsMatching)
                return false;

            if (gridCell.IsLocked || gridCell.IsMoving)
                return false;

            return true;
        }

        public bool IsMatchable(Vector3Int position, out MatchResult matchResult)
        {
            return _matchRule.CheckMatch(position, out matchResult);
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        private void PrintMatch(MatchResult match)
        {
#if UNITY_EDITOR
            StringBuilder builder = new();
            string color = GetColor(match.CandyColor);

            builder.Append($"<b>Match:</b> {match.MatchSequence.Count}.   ");
            builder.Append($"<b>Type:</b> {match.MatchType}.    ");
            builder.Append($"<b>Color:</b> <color={color}><b>{match.CandyColor}</b></color>.    ");
            builder.Append($"<b>Pivot:</b> {match.Position}.    ");
            builder.Append($"<b>Positions:</b> ");
            builder.Append("{ ");

            for (int i = 0; i < match.MatchSequence.Count; i++)
                builder.Append($"<b>{i + 1}:</b> {match.MatchSequence[i]}; ");

            builder.Append(" }");
            Debug.Log(builder.ToString());
            builder.Clear();
#endif
        }

        private string GetColor(CandyColor candyColor)
        {
            string colorCode = "";

            colorCode = candyColor switch
            {
                CandyColor.Red => "#F73540",
                CandyColor.Green => "#47D112",
                CandyColor.Orange => "#FA8500",
                CandyColor.Purple => "#CE2AEF",
                CandyColor.Yellow => "#F8D42A",
                CandyColor.Blue => "#23AAFB",
                _ => "#000000"
            };

            return colorCode;
        }

        public void Dispose()
        {
            _cts.Dispose();
            _adjacentSteps.Clear();
            _disposable.Dispose();
        }
    }
}
