using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        private readonly MatchItemsTask _matchItemsTask;
        private readonly InputProcessTask _inputProcessor;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MoveItemTask _moveItemTask;
        private readonly SwapItemTask _swapItemTask;
        private readonly SpawnItemTask _spawnItemTask;
        private readonly ActivateBoosterTask _activateBoosterTask;

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager, ItemManager itemManager, SpawnItemTask spawnItemTask
            , MatchItemsTask matchItemsTask, MetaItemManager metaItemManager, BreakGridTask breakGridTask)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _swapItemTask = new(_gridCellManager, _matchItemsTask);
            _inputProcessor = new(boardInput, _gridCellManager, _swapItemTask);
            _inputProcessor.AddTo(ref builder);

            _breakGridTask = breakGridTask;
            _moveItemTask = new(_gridCellManager, _breakGridTask);
            _moveItemTask.AddTo(ref builder);

            _activateBoosterTask = new(_gridCellManager, _breakGridTask);
            _activateBoosterTask.AddTo(ref builder);

            _breakGridTask.SetActivateBoosterTask(_activateBoosterTask);
            _swapItemTask.SetActivateBoosterTask(_activateBoosterTask);

            _spawnItemTask = spawnItemTask;
            _checkGridTask = new(_gridCellManager, _moveItemTask, _spawnItemTask, _matchItemsTask);
            _checkGridTask.AddTo(ref builder);

            SetCheckGridTask();
            _disposable = builder.Build();
        }

        public void SetInputActive(bool isActive)
        {
            _inputProcessor.IsActive = isActive;
        }

        private void SetCheckGridTask()
        {
            _moveItemTask.SetCheckGridTask(_checkGridTask);
            _breakGridTask.SetCheckGridTask(_checkGridTask);
            _matchItemsTask.SetCheckGridTask(_checkGridTask);
            _spawnItemTask.SetCheckGridTask(_checkGridTask);
            _activateBoosterTask.SetCheckGridTask(_checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
