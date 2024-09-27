using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Sirenix.OdinInspector;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using GlobalScripts.Extensions;
using System.Linq;
using CandyMatch3.Scripts.Common.Enums;

public class TestRegion : MonoBehaviour
{
    public Vector3Int StartPosition;
    public List<Vector3Int> Positions;
    private Direction _direction;

    public void TestPivot(MatchableRegion region)
    {
        BoundsInt range = region.GetRegionBounds();
        HashSet<Vector3Int> checkPositions = new();
        List<Vector3Int> poses = range.Iterator2D(BoundsExtension.SortOrder.Descending).ToList();

        foreach (Vector3Int position in range.Iterator2D(BoundsExtension.SortOrder.Descending))
        {
            if (!region.IsInRegion(position))
                continue;

            if (!region.IsMatchPivot(position))
                continue;

            region.AddPivot(position);
        }

        StartPosition = region.TakePivot(false);
    }

    [Button]
    public void TestRegionMatch()
    {
        int extendCount = 0;
        MatchableRegion region = new();

        region.Elements = Positions.ToHashSet();
        HashSet<Vector3Int> extendPositions = new();
        TestPivot(region);

        int pivotCount = region.Pivotables.Count;
        for (int i = 0; i < pivotCount; i++)
        {
            Vector3Int pivot = region.TakePivot(true);
            Debug.Log($"Pivot: {pivot}");
            if (!region.IsInRegion(pivot))
                continue;

            ExtendPosition(region, pivot, ref extendCount, Direction.None, true, extendPositions, ref _direction);
            foreach (var item in region.Elements)
            {
                Debug.Log(item);
            }

            region.RemoveElement(pivot);
            region.RemoveRange(extendPositions);
        }

    }

    private Direction GetDirection(Vector3Int dir)
    {
        if (dir == Vector3Int.up || dir == Vector3Int.down)
            return Direction.Vertical;
        else
            return Direction.Horizontal;
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
}
