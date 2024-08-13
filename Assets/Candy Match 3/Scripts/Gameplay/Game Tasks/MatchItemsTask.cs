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

        public bool CheckMatch(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell.HasItem && gridCell.BlockItem.IsMatchable)
            {
                bool isMatch = _matchModel.CheckMatch(position, out MatchResult matchResult);

                if (isMatch)
                    _breakGridTask.BreakMatch(matchResult).Forget();

                return isMatch;
            }

            return false;
        }

        public void Match(Vector3Int position)
        {
            MatchResult matchResult;
            bool isMatch = _matchModel.CheckMatch(position, out matchResult);

            if (isMatch)
            {

            }
        }

        public bool CheckMatchAtPosition(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);
            return gridCell.HasItem && gridCell.BlockItem.IsMatchable;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
