using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match4Model : BaseMatchModel
    {
        public override MatchType MatchType => MatchType.Match4;

        public Match4Model(GridCellManager gridCellManager) : base(gridCellManager) { }

        protected override List<SequencePosition> matchCellPositions => new()
        {
            new(new() { new(1, 0), new(2, 0), new(3, 0) }), // use for falling check
            new(new() { new(-1, 0), new(1, 0), new(2, 0) }),
        };

        public override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            List<IGridCell> matchCells = new();
            if(inDirection == Vector3Int.down || inDirection == Vector3Int.up)
            {
                matchCells = GetMatchCellSFromSequence(gridPosition, matchCellPositions[1], 0);
                if (matchCells.Count >= 3)
                    return matchCells;

                else
                {
                    matchCells = GetMatchCellSFromSequence(gridPosition, matchCellPositions[1], 180);
                    if (matchCells.Count >= 3)
                        return matchCells;
                }
            }

            else if (inDirection == Vector3Int.left || inDirection == Vector3Int.right)
            {
                matchCells = GetMatchCellSFromSequence(gridPosition, matchCellPositions[1], 90);
                if (matchCells.Count >= 3)
                    return matchCells;

                else
                {
                    matchCells = GetMatchCellSFromSequence(gridPosition, matchCellPositions[1], -90);
                    if (matchCells.Count >= 3)
                        return matchCells;
                }
            }

            return matchCells;
        }
    }
}
