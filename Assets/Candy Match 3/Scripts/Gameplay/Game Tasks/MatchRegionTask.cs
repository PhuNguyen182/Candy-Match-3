using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Common.Constants;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MatchRegionTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly FindItemRegionTask _findItemRegionTask;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly SuggestTask _suggestTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;

        public MatchRegionTask(GridCellManager gridCellManager, FindItemRegionTask findItemRegionTask, MatchItemsTask matchItemsTask, SuggestTask suggestTask)
        {
            _gridCellManager = gridCellManager;
            _findItemRegionTask = findItemRegionTask;
            _matchItemsTask = matchItemsTask;
            _suggestTask = suggestTask;

            _cts = new();
            _token = _cts.Token;
        }

        public void CheckMatchRegion(Vector3Int position)
        {
            _findItemRegionTask.CheckMatchRegion(position);
        }

        public async UniTask<int> MatchAllRegions()
        {
            using(ListPool<MatchableRegion>.Get(out List<MatchableRegion> regions))
            {
                SetSuggestActive(false);
                // Must wait until all matches are solved to check new matchs to prevent duplicate check match
                await UniTask.WaitUntil(() => _findItemRegionTask.RegionCount <= 0
                                        , PlayerLoopTiming.FixedUpdate, _token);
                regions = _findItemRegionTask.CollectMatchableRegions();

                if (regions.Count <= 0)
                {
                    _findItemRegionTask.Cleanup();
                    SetSuggestActive(true);
                    return 0;
                }

                int count = 0;
                _checkGridTask.CanCheck = false;
                _findItemRegionTask.ClearCheckPositions();

                using (ListPool<UniTask>.Get(out List<UniTask> matchTasks))
                {
                    for (int i = 0; i < regions.Count; i++)
                    {
                        using (ListPool<MatchRegionResult>.Get(out var results))
                        {
                            results = GetMatchRegionResult(regions[i]);
                            for (int j = 0; j < results.Count; j++)
                            {
                                using (results[j])
                                {
                                    count = count + 1;
                                    matchTasks.Add(_matchItemsTask.ProcessRegionMatch(results[j]));
                                }
                            }
                        }
                    }

                    await UniTask.WhenAll(matchTasks);
                }

                _findItemRegionTask.ClearRegions();
                _checkGridTask.CanCheck = true;
                SetSuggestActive(true);
                return count;
            }
        }

        private List<MatchRegionResult> GetMatchRegionResult(MatchableRegion region)
        {
            using (region)
            {
                int maxExtendedCount = 0;
                Direction mainDirection = Direction.None;
                List<MatchRegionResult> results = new();

                BoundsInt range = region.GetRegionBounds();
                HashSet<Vector3Int> checkPositions = new();

                var positions = range.Iterator2D(BoundsExtension.SortOrder.Descending
                                                , BoundsExtension.AxisOrder.YX);
                foreach (Vector3Int position in positions)
                {
                    if (!region.IsInRegion(position))
                        continue;

                    if (!region.IsPivotable(position))
                        continue;

                    // Collect all pivotable positions
                    region.AddPivot(position);
                }

                int pivotCount = region.Pivotables.Count;
                for (int i = 0; i < pivotCount; i++)
                {
                    // In all pivotable check matchable regions can be splitted
                    Vector3Int pivot = region.TakePivot(true);
                    if (!region.IsInRegion(pivot))
                        continue;

                    maxExtendedCount = 0;
                    checkPositions.Clear();
                    ExtendPosition(region, pivot, ref maxExtendedCount, Direction.None
                                  , true, checkPositions, ref mainDirection);

                    checkPositions.Add(pivot);
                    MatchType matchType = GetMatchType(checkPositions.Count, maxExtendedCount, mainDirection);

                    MatchRegionResult result = new()
                    {
                        MatchType = matchType,
                        CandyColor = region.RegionColor,
                        PivotPosition = pivot,
                        Positions = new(checkPositions)
                    };

                    region.RemoveElement(pivot);
                    region.RemoveRange(checkPositions);
                    result.CheckArea = range;
                    results.Add(result);
                }

                return results;
            }
        }

        private void ExtendPosition(MatchableRegion region, Vector3Int position, ref int maxExtendedCount
            , Direction inDirection, bool isFirst, HashSet<Vector3Int> extendPositions, ref Direction outDirection)
        {
            Direction direction;

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                if (isFirst)
                {
                    for (int i = 0; i < Match3Constants.AdjacentSteps.Count; i++)
                    {
                        Vector3Int step = Match3Constants.AdjacentSteps[i];
                        positions = region.GetExtendedPositions(position, step);

                        if (positions.Count < 2)
                            continue;

                        direction = GetDirection(step);
                        extendPositions.AddRange(positions);
                        outDirection = direction;

                        if (positions.Count > maxExtendedCount)
                            maxExtendedCount = positions.Count;

                        for (int j = 0; j < positions.Count; j++)
                        {
                            ExtendPosition(region, positions[j], ref maxExtendedCount, direction
                                          , false, extendPositions, ref outDirection);
                        }
                    }
                }

                else
                {
                    if (inDirection == Direction.Vertical)
                    {
                        positions = region.GetHorizontalExtendedPositions(position);

                        if (positions.Count > 1)
                        {
                            if (positions.Count > maxExtendedCount)
                            {
                                maxExtendedCount = positions.Count;
                                outDirection = Direction.Horizontal;
                            }

                            for (int i = 0; i < positions.Count; i++)
                            {
                                if (!extendPositions.Contains(positions[i]))
                                {
                                    extendPositions.Add(positions[i]);
                                    ExtendPosition(region, positions[i], ref maxExtendedCount, Direction.Horizontal
                                                  , false, extendPositions, ref outDirection);
                                }
                            }
                        }
                    }

                    else
                    {
                        positions = region.GetVerticalExtendedPositions(position);

                        if (positions.Count > 1)
                        {
                            if (positions.Count > maxExtendedCount)
                            {
                                maxExtendedCount = positions.Count;
                                outDirection = Direction.Vertical;
                            }

                            for (int i = 0; i < positions.Count; i++)
                            {
                                if (!extendPositions.Contains(positions[i]))
                                {
                                    extendPositions.Add(positions[i]);
                                    ExtendPosition(region, positions[i], ref maxExtendedCount, Direction.Vertical
                                                  , false, extendPositions, ref outDirection);
                                }
                            }
                        }
                    }
                }
            }
        }

        private Direction GetDirection(Vector3Int dir)
        {
            if (dir == Vector3Int.up || dir == Vector3Int.down)
                return Direction.Vertical;
            
            else if(dir == Vector3Int.left || dir == Vector3Int.right)
                return Direction.Horizontal;
            
            return Direction.None;
        }

        private MatchType GetMatchType(int totalItemCount, int maxExtendCount, Direction direction)
        {
            MatchType matchType = MatchType.None;

            if(totalItemCount == 3 || totalItemCount == 4)
            {
                if (maxExtendCount < 3)
                    matchType = MatchType.Match3;

                else if (maxExtendCount == 3)
                    matchType = direction == Direction.Horizontal ? MatchType.Match4Vertical : MatchType.Match4Horizontal;
            }

            else if(totalItemCount >= 5)
            {
                if (maxExtendCount < 3 && maxExtendCount > 1)
                    matchType = MatchType.MatchT;

                else if (maxExtendCount >= 3)
                    matchType = MatchType.Match5;
            }

            return matchType;
        }

        private void SetSuggestActive(bool active)
        {
            if (!active)
            {
                _suggestTask.Suggest(false);
                _suggestTask.ClearSuggest();
            }

            _suggestTask.IsActive = active;
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
