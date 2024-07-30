using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchTModel : BaseMatchModel
    {
        /*
         * Default:
         *      X O X
         *        X
         *        X
         */
        protected override List<Vector3Int> matchCellPositions => new()
        {
            new(0, -1), new(1, 0), new(0, -1), new(0, 2)
        };

        public override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            return null;
        }
    }
}
