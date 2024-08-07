using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        private readonly InputProcessor _inputProcessor;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MoveItemTask _moveItemTask;

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;

            _inputProcessor = new(boardInput, _gridCellManager);
            _inputProcessor.AddTo(ref builder);

            _breakGridTask = new(_gridCellManager);
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
