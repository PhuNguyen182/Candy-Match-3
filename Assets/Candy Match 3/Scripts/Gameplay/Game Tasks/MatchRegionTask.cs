using System;
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

        public MatchRegionTask(GridCellManager gridCellManager, FindItemRegionTask findItemRegionTask, MatchItemsTask matchItemsTask)
        {
            _gridCellManager = gridCellManager;
            _findItemRegionTask = findItemRegionTask;
            _matchItemsTask = matchItemsTask;
        }

        public void CheckMatchRegion(Vector3Int position)
        {
            _findItemRegionTask.CheckRegion(position);
        }

        public async UniTask MatchAllRegions()
        {
            using(ListPool<MatchableRegion>.Get(out List<MatchableRegion> regions))
            {
                regions = _findItemRegionTask.CollectMatchableRegions();

                using (ListPool<UniTask>.Get(out List<UniTask> matchTasks))
                {
                    for (int i = 0; i < regions.Count; i++)
                    {
                        using MatchRegionResult result = GetMatchRegionResult(regions[i]);
                        matchTasks.Add(_matchItemsTask.ProcessMatch(result));
                    }

                    await UniTask.WhenAll(matchTasks);
                }
            }
        }

        private MatchRegionResult GetMatchRegionResult(MatchableRegion region)
        {
            using (region)
            {
                int maxExtendedCount = 0;
                Direction mainDirection = Direction.None;

                Vector3Int pivotPosition = Vector3Int.zero;
                BoundsInt range = region.GetRegionBounds();
                HashSet<Vector3Int> checkPositions = new();

                foreach (Vector3Int position in range.Iterator2D(BoundsExtension.SortOrder.Descending))
                {
                    if (!region.IsInRegion(position))
                        continue;

                    if (region.GetExtendablePositionCount(position) < 2)
                        continue;

                    pivotPosition = position;
                    break;
                }

                ExtendPosition(region, pivotPosition, ref maxExtendedCount, Direction.None
                              , true, checkPositions, ref mainDirection);

                checkPositions.Add(pivotPosition);
                MatchType matchType = GetMatchType(checkPositions.Count, maxExtendedCount, mainDirection);

                MatchRegionResult result = new()
                {
                    MatchType = matchType,
                    CandyColor = region.RegionColor,
                    PivotPosition = pivotPosition,
                    Positions = checkPositions
                };

                return result;
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

            if(totalItemCount >= 3 && totalItemCount <= 4)
            {
                if (maxExtendCount == 2)
                    matchType = MatchType.Match3;

                else if (maxExtendCount == 3)
                    matchType = direction == Direction.Horizontal ? MatchType.Match4Vertical : MatchType.Match4Horizontal;
            }

            else
            {
                if (maxExtendCount < 3)
                    matchType = MatchType.MatchT;
                
                else
                    matchType = MatchType.Match5;
            }

            return matchType;
        }

        public void Dispose()
        {

        }
    }
}
