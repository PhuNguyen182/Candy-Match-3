using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match5Model : BaseMatchModel
    {
        public override MatchType MatchType => MatchType.Match5;
        protected override List<SequencePosition> matchCellPositions { get; }

        public Match5Model(GridCellManager gridCellManager) : base(gridCellManager)
        {
            matchCellPositions = new()
            {
                new(new() { new(1, 0), new(2, 0), new(3, 0), new(4, 0) }), // use for falling check
                new(new() { new(-2, 0), new(-1, 0), new(1, 0), new(2, 0) }),
            };
        }

        protected override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            List<IGridCell> matchCells = new();
            int minMatchCount = GetMinMatchCount();

            if (inDirection == Vector3Int.down || inDirection == Vector3Int.up)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[1], 0);
                if (matchCells.Count >= minMatchCount)
                    return matchCells;
            }

            else if (inDirection == Vector3Int.left || inDirection == Vector3Int.right)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[1], 90);
                if (matchCells.Count >= minMatchCount)
                    return matchCells;
            }

            return matchCells;
        }
    }
}
