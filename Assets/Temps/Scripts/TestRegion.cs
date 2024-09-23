using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Button]
    public void TestRegionMatch()
    {
        int extendCount = 0;
        MatchableRegion region = new();

        region.Elements = Positions.ToHashSet();
        HashSet<Vector3Int> extendPositions = new();
        ExtendPosition(region, StartPosition, ref extendCount, Direction.None, true, extendPositions);

        Debug.Log($"Max count: {extendCount} {_direction}");
        foreach (Vector3Int position in extendPositions)
        {
            Debug.Log(position);
        }
    }

    private Direction GetDirection(Vector3Int dir)
    {
        if (dir == Vector3Int.up || dir == Vector3Int.down)
            return Direction.Vertical;
        else
            return Direction.Horizontal;
    }

    private void ExtendPosition(MatchableRegion region, Vector3Int position, ref int maxExtendedCount, Direction inDirection, bool isFirst, HashSet<Vector3Int> extendPositions)
    {
        List<Vector3Int> positions = new();
        Direction direction;

        if (isFirst)
        {
            for (int i = 0; i < Match3Constants.AdjacentSteps.Count; i++)
            {
                Vector3Int step = Match3Constants.AdjacentSteps[i];
                positions = region.GetExtendedPositions(position, step);

                if (positions.Count < 2)
                    continue;

                direction = GetDirection(step);
                _direction = direction;
                extendPositions.AddRange(positions);
                if (positions.Count > maxExtendedCount)
                    maxExtendedCount = positions.Count;

                for (int j = 0; j < positions.Count; j++)
                {
                    ExtendPosition(region, positions[j], ref maxExtendedCount, direction, false, extendPositions);
                }
            }
        }

        else
        {
            // Check horizontal
            if (inDirection == Direction.Vertical)
            {
                positions = region.GetHorizontalExtendedPositions(position);
                if (positions.Count > 1)
                {
                    if (positions.Count > maxExtendedCount)
                    {
                        maxExtendedCount = positions.Count;
                        _direction = Direction.Horizontal;
                    }

                    for (int i = 0; i < positions.Count; i++)
                    {
                        if (!extendPositions.Contains(positions[i]))
                        {
                            extendPositions.Add(positions[i]);
                            ExtendPosition(region, positions[i], ref maxExtendedCount, Direction.Horizontal, false, extendPositions);
                        }
                    }
                }
            }

            else
            {
                // Check vertical
                positions = region.GetVerticalExtendedPositions(position);
                if (positions.Count > 1)
                {
                    if (positions.Count > maxExtendedCount)
                    {
                        maxExtendedCount = positions.Count;
                        _direction = Direction.Vertical;
                    }

                    for (int i = 0; i < positions.Count; i++)
                    {
                        if (!extendPositions.Contains(positions[i]))
                        {
                            extendPositions.Add(positions[i]);
                            ExtendPosition(region, positions[i], ref maxExtendedCount, Direction.Vertical, false, extendPositions);
                        }
                    }
                }
            }
        }
    }
}
