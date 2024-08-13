using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchLModel : BaseMatchModel
    {
        public override MatchType MatchType => MatchType.MatchL;
        protected override List<SequencePosition> matchCellPositions { get; }

        public MatchLModel(GridCellManager gridCellManager) : base(gridCellManager)
        {
            matchCellPositions = new()
            {
                new(new() { new(1, 0), new(2, 0), new(0, 1), new(0, 2), new(-1, 0), new(0, -1) }),
            };

            for (int i = 1; i < checkAngles.Length; i++)
            {
                List<Vector3Int> sequence = GetRotatePositions(matchCellPositions[0].Sequence, checkAngles[i]);
                matchCellPositions.Add(new(sequence));
            }
        }
    }
}
