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
        private int[] _checkAngles = new[] { 0, 90, 180, -90 };

        public override MatchType MatchType => MatchType.Match3;
        protected override List<SequencePosition> matchCellPositions { get; }

        public Match3Model(GridCellManager gridCellManager) : base(gridCellManager)
        {
            matchCellPositions = new()
            {
                new(new() { new(-1, 0), new(1, 0) }),
                new(new() { new(1, 0), new(2, 0) }),
            };
        }

        protected override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            List<IGridCell> matchGrids = new();
            int minMatchCount = GetMinMatchCount();

            for (int i = 0; i < matchCellPositions.Count; i++)
            {
                for (int j = 0; j < _checkAngles.Length; j++)
                {
                    matchGrids = GetMatchCellsFromSequence(gridPosition, matchCellPositions[i], _checkAngles[j]);
                    if (matchGrids.Count >= minMatchCount)
                        return matchGrids;
                }
            }

            return matchGrids;
        }
    }
}
