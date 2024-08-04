using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MoveItemTask _moveItemTask;

        private IDisposable _disposable;

        public GameTaskManager(GridCellManager gridCellManager)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;

            _breakGridTask = new();
            _checkGridTask = new(_gridCellManager);            
            _moveItemTask = new(_gridCellManager, _checkGridTask);

            _disposable = builder.Build();
        }

        public void CheckMoveOnStart()
        {
            _moveItemTask.MoveItems().Forget();
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
