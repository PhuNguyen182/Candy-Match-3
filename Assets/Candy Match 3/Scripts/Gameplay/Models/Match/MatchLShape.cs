using CandyMatch3.Scripts.Gameplay.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchLShape : BaseMatchModel
    {
        /*
         * Default:
         *      X
         *      X  
         *      O X X
         */

        protected override List<Vector3Int> matchCellPositions => new()
        {
            new(1, 0), new(2, 0), new(0, 1), new(0, 2)
        };

        public override List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection)
        {
            return null;
        }
    }
}
