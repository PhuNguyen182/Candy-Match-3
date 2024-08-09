using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchModel : IDisposable
    {
        private readonly Match3Model _match3Model;
        private readonly Match4Model _match4Model;
        private readonly Match5Model _match5Model;
        private readonly MatchLShape _matchLModel;
        private readonly MatchTModel _matchTModel;

        private IDisposable _disposable;

        public MatchModel(GridCellManager gridCellManager)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _match3Model = new(gridCellManager);
            _match4Model = new(gridCellManager);
            _match5Model = new(gridCellManager);
            _matchLModel = new(gridCellManager);
            _matchTModel = new(gridCellManager);
            
            _match3Model.AddTo(ref builder);
            _match4Model.AddTo(ref builder);
            _match5Model.AddTo(ref builder);
            _matchLModel.AddTo(ref builder);
            _matchTModel.AddTo(ref builder);

            _disposable = builder.Build();
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

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
