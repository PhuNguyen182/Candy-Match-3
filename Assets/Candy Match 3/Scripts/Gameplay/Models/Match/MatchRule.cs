using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class MatchRule : IDisposable
    {
        private readonly Match3Model _match3Model;
        private readonly Match4HorizontalModel _match4HorizontalModel;
        private readonly Match4VerticalModel _match4VerticalModel;
        private readonly Match5Model _match5Model;
        private readonly MatchLModel _matchLModel;
        private readonly MatchTModel _matchTModel;

        private IDisposable _disposable;

        public MatchRule(GridCellManager gridCellManager)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _match3Model = new(gridCellManager);
            _match4HorizontalModel = new(gridCellManager);
            _match4VerticalModel = new(gridCellManager);
            _match5Model = new(gridCellManager);
            _matchLModel = new(gridCellManager);
            _matchTModel = new(gridCellManager);
            
            _match3Model.AddTo(ref builder);
            _match4HorizontalModel.AddTo(ref builder);
            _match4VerticalModel.AddTo(ref builder);
            _match5Model.AddTo(ref builder);
            _matchLModel.AddTo(ref builder);
            _matchTModel.AddTo(ref builder);

            _disposable = builder.Build();
        }

        public bool CheckMatch(Vector3Int checkPosition, out MatchResult matchResult)
        {
            if (_match5Model.CheckMatch(checkPosition, out matchResult))
                return true;

            else if (_matchLModel.CheckMatch(checkPosition, out matchResult))
                return true;

            else if (_matchTModel.CheckMatch(checkPosition, out matchResult))
                return true;

            else if(_match4HorizontalModel.CheckMatch(checkPosition, out matchResult))
                return true;
            
            else if(_match4VerticalModel.CheckMatch(checkPosition, out matchResult))
                return true;

            else if (_match3Model.CheckMatch(checkPosition, out matchResult))
                return true;

            return false;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
