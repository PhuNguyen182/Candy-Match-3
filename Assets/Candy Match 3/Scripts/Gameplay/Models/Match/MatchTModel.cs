using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchTModel : BaseMatchModel
    {
        public override MatchType MatchType => MatchType.MatchT;
        protected override List<SequencePosition> matchCellPositions { get; }

        public MatchTModel(GridCellManager gridCellManager) : base(gridCellManager)
        {
            matchCellPositions = new()
            {
                new(new() { new(0, -1), new(1, 0), new(0, -1), new(0, -2) }),
            };
        }

        protected override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            List<IGridCell> matchCells = new();

            if (inDirection == Vector3Int.down)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[0], 0);
                if (matchCells.Count >= 4)
                    return matchCells;
            }

            else if (inDirection == Vector3Int.up)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[0], 180);
                if (matchCells.Count >= 4)
                    return matchCells;
            }

            else if (inDirection == Vector3Int.right)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[0], 90);
                if (matchCells.Count >= 4)
                    return matchCells;
            }

            else if (inDirection == Vector3Int.left)
            {
                matchCells = GetMatchCellsFromSequence(gridPosition, matchCellPositions[0], -90);
                if (matchCells.Count >= 4)
                    return matchCells;
            }

            return matchCells;
        }
    }
}
