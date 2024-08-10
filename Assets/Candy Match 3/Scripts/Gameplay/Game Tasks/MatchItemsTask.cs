using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class MatchItemsTask : IDisposable
    {
        private readonly MatchModel _matchModel;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;

        private IDisposable _disposable;

        public MatchItemsTask(GridCellManager gridCellManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            _matchModel = new(_gridCellManager);
            _breakGridTask = breakGridTask;

            DisposableBuilder builder = Disposable.CreateBuilder();
            _matchModel.AddTo(ref builder);
            _disposable = builder.Build();
        }

        public async UniTask<bool> CheckMatch(Vector3Int position, Vector3Int inDirection)
        {
            MatchType matchType;
            List<IGridCell> matchedCells;
            bool isMatch = _matchModel.CheckMatch(position, inDirection, out matchedCells, out matchType);

            if (isMatch)
                await _breakGridTask.BreakMatch(matchedCells, matchType);

            return isMatch;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
