using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match5Model : BaseMatchModel
    {
        protected override int matchScoreCount => 4;
        protected override int requiredItemCount => 4;
        public override MatchType MatchType => MatchType.Match5;
        protected override List<SequencePattern> sequencePattern { get; }

        public Match5Model(GridCellManager gridCellManager) : base(gridCellManager)
        {
            sequencePattern = new()
            {
                new(new() { new(-2, 0), new(-1, 0), new(1, 0), new(2, 0) }),
            };

            for (int i = 1; i < checkAngles.Length; i++)
            {
                List<Vector3Int> sequence1 = GetRotatePositions(sequencePattern[0].Pattern, checkAngles[i]);
                sequencePattern.Add(new(sequence1));
            }

            OnConstuctor();
        }
    }
}
