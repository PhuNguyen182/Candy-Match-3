using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchTModel : BaseMatchModel
    {
        protected override int requiredItemCount => 4;
        public override MatchType MatchType => MatchType.MatchT;
        protected override List<SequencePattern> sequencePattern { get; }

        public MatchTModel(GridCellManager gridCellManager, ItemManager itemManager) : base(gridCellManager, itemManager)
        {
            sequencePattern = new()
            {
                new(new() { new(-1, 0), new(1, 0), new(0, -1), new(0, -2) }),
            };

            for (int i = 1; i < checkAngles.Length; i++)
            {
                List<Vector3Int> sequence = GetRotatePositions(sequencePattern[0].Pattern, checkAngles[i]);
                sequencePattern.Add(new(sequence));
            }

            OnConstuctor();
        }
    }
}
