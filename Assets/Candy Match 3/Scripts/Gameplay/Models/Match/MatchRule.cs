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
            _match3Model.AddTo(ref builder);

            _match4HorizontalModel = new(gridCellManager);
            _match4HorizontalModel.AddTo(ref builder);
            
            _match4VerticalModel = new(gridCellManager);
            _match4VerticalModel.AddTo(ref builder);
            
            _match5Model = new(gridCellManager);
            _match5Model.AddTo(ref builder);
            
            _matchLModel = new(gridCellManager);
            _matchLModel.AddTo(ref builder);
            
            _matchTModel = new(gridCellManager);
            _matchTModel.AddTo(ref builder);

            _disposable = builder.Build();
        }

        public bool CheckMatch(Vector3Int checkPosition, out MatchResult matchResult)
        {
            int matchScore = 0;
            BaseMatchModel matchModel = null;

            if (_match5Model.CheckMatch(checkPosition, out matchScore))
                matchModel = _match5Model;

            else if (_matchLModel.CheckMatch(checkPosition, out matchScore))
                matchModel = _matchLModel;

            else if (_matchTModel.CheckMatch(checkPosition, out matchScore))
                matchModel = _matchTModel;

            else if (_match4HorizontalModel.CheckMatch(checkPosition, out matchScore))
                matchModel = _match4HorizontalModel;

            else if (_match4VerticalModel.CheckMatch(checkPosition, out matchScore))
                matchModel = _match4VerticalModel;

            else if (_match3Model.CheckMatch(checkPosition, out matchScore))
                matchModel = _match3Model;

            if(matchModel != null)
            {
                matchResult = matchModel.GetMatchResult(checkPosition);
                return true;
            }

            matchResult = new() { MatchSequence = new() };
            return false;
        }

        public bool CheckMatch(Vector3Int checkPosition, out int matchScore)
        {
            if (_match5Model.CheckMatch(checkPosition, out matchScore))
                return true;

            else if (_matchLModel.CheckMatch(checkPosition, out matchScore))
                return true;

            else if (_matchTModel.CheckMatch(checkPosition, out matchScore))
                return true;

            else if (_match4HorizontalModel.CheckMatch(checkPosition, out matchScore))
                return true;

            else if (_match4VerticalModel.CheckMatch(checkPosition, out matchScore))
                return true;

            else if (_match3Model.CheckMatch(checkPosition, out matchScore))
                return true;

            return false;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
