using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match3Model : BaseMatchModel
    {
        public override MatchType MatchType => MatchType.Match3;
        protected override List<SequencePosition> matchCellPositions { get; }

        public Match3Model(GridCellManager gridCellManager) : base(gridCellManager)
        {
            matchCellPositions = new()
            {
                new(new() { new(-1, 0), new(1, 0) }),
                new(new() { new(1, 0), new(2, 0) }),
                new(new() { new(-2, 0), new(-1, 0) }),
            };
        }

        public override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            List<IGridCell> matchGrids = new();
            for (int i = 0; i < matchCellPositions.Count; i++)
            {
                matchGrids = GetMatchCellsFromSequence(gridPosition, matchCellPositions[i], 0);
                if (matchGrids.Count >= 2)
                    break;

                else
                {
                    matchGrids = GetMatchCellsFromSequence(gridPosition, matchCellPositions[i], 90);
                    if (matchGrids.Count >= 2)
                        break;
                }
            }

            return matchGrids;
        }
    }
}
