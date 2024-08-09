using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchModel
    {
        private readonly Match3Model _match3Model;
        private readonly Match4Model _match4Model;
        private readonly Match5Model _match5Model;
        private readonly MatchLShape _matchLModel;
        private readonly MatchTModel _matchTModel;

        public MatchModel(GridCellManager gridCellManager)
        {
            _match3Model = new(gridCellManager);
            _match4Model = new(gridCellManager);
            _match5Model = new(gridCellManager);
            _matchLModel = new(gridCellManager);
            _matchTModel = new(gridCellManager);
        }

        public bool CheckMatch(Vector3Int checkPosition, Vector3Int inDirection, out List<IGridCell> matchedCells)
        {
            if (_match5Model.CheckMatch(checkPosition, inDirection, out matchedCells))
                return true;

            if(_match4Model.CheckMatch(checkPosition, inDirection, out matchedCells))
                return true;

            if (_match3Model.CheckMatch(checkPosition, inDirection, out matchedCells))
                return true;

            if (_matchLModel.CheckMatch(checkPosition, inDirection, out matchedCells))
                return true;

            if (_matchTModel.CheckMatch(checkPosition, inDirection, out matchedCells))
                return true;

            matchedCells = new();
            return false;
        }
    }
}
