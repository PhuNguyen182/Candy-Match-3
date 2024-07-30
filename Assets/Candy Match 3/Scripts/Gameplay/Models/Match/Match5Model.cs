using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class Match5Model : BaseMatchModel
    {
        /*
         * Default:
         *     X X O X X
         */
        protected override List<Vector3Int> matchCellPositions => new()
        {
            new(-2, 0), new(-1, 0), new(1, 0), new(2, 0)
        };

        public override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            return null;
        }
    }
}
