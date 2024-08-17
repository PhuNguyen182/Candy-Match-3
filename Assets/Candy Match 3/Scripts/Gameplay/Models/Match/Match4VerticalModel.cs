using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match4VerticalModel : BaseMatchModel
    {
        protected override int minMatchCount => 3;
        public override MatchType MatchType => MatchType.Match4Vertical;

        public Match4VerticalModel(GridCellManager gridCellManager) : base(gridCellManager)
        {
            sequencePattern = new()
            {
                new(new() { new(0, -1), new(0, 1), new(0, 2) }),
                new(new() { new(0, 1), new(0, 2), new(0, 3) }), // use for falling check
            };

            List<Vector3Int> sequence1 = GetRotatePositions(sequencePattern[0].Pattern, 180);
            List<Vector3Int> sequence2 = GetRotatePositions(sequencePattern[1].Pattern, 180);

            sequencePattern.Add(new(sequence1));
            sequencePattern.Add(new(sequence2));
        }

        protected override List<SequencePattern> sequencePattern { get; }
    }
}
