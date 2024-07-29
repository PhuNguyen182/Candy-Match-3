using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Models.MatchModels
{
    public abstract class BaseMatchModel
    {
        protected abstract List<Vector3Int> matchCellPositions { get; set; }

        public abstract bool CheckMatch();
    }
}
