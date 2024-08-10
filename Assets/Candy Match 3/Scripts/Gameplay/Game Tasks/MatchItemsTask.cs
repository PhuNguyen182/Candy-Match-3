using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MatchItemsTask : IDisposable
    {
        private readonly MatchModel _matchModel;
        private readonly GridCellManager _gridCellManager;

        private IDisposable _disposable;

        public MatchItemsTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
            _matchModel = new(_gridCellManager);

            DisposableBuilder builder = Disposable.CreateBuilder();
            _matchModel.AddTo(ref builder);
            _disposable = builder.Build();
        }

        public bool CheckMatch(Vector3Int position, Vector3Int inDirection, out List<IGridCell> matchedCells, out MatchType matchType)
        {            
            return _matchModel.CheckMatch(position, inDirection, out matchedCells, out matchType);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
