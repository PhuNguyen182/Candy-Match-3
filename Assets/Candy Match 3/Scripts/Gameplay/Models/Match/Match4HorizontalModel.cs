using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match4HorizontalModel : BaseMatchModel
    {
        protected override int matchScoreCount => 2;
        protected override int requiredItemCount => 3;
        public override MatchType MatchType => MatchType.Match4Horizontal;

        public Match4HorizontalModel(GridCellManager gridCellManager) : base(gridCellManager)
        {
            sequencePattern = new()
            {
                new(new() { new(-1, 0), new(1, 0), new(2, 0) }),
                new(new() { new(1, 0), new(2, 0), new(3, 0) }), // use for falling check
            };

            List<Vector3Int> sequence1 = GetRotatePositions(sequencePattern[0].Pattern, 180);
            List<Vector3Int> sequence2 = GetRotatePositions(sequencePattern[1].Pattern, 180);

            sequencePattern.Add(new(sequence1));
            sequencePattern.Add(new(sequence2));

            OnConstuctor();
        }

        protected override List<SequencePattern> sequencePattern { get; }
    }
}
