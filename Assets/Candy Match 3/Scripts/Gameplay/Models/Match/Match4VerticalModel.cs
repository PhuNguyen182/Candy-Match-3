using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match4VerticalModel : BaseMatchModel
    {
        protected override int matchScoreCount => 2;
        protected override int requiredItemCount => 3;
        public override MatchType MatchType => MatchType.Match4Vertical;

        public Match4VerticalModel(GridCellManager gridCellManager) : base(gridCellManager)
        {
            sequencePattern = new()
            {
                new(new() { new(0, -1), new(0, 1), new(0, 2) }),
            };

            List<Vector3Int> sequence1 = GetRotatePositions(sequencePattern[0].Pattern, 180);
            sequencePattern.Add(new(sequence1));

            OnConstuctor();
        }

        protected override List<SequencePattern> sequencePattern { get; }
    }
}
