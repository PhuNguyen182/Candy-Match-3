using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Common.Databases;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        private readonly MoveItemTask _moveItemTask;
        private readonly SwapItemTask _swapItemTask;
        private readonly SpawnItemTask _spawnItemTask;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly InputProcessTask _inputProcessor;
        private readonly GridCellManager _gridCellManager;
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly ActivateBoosterTask _activateBoosterTask;
        private readonly ComboBoosterHandleTask _comboBoosterHandleTask;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly SuggestTask _suggestTask;

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager, ItemManager itemManager, SpawnItemTask spawnItemTask
            , MatchItemsTask matchItemsTask, MetaItemManager metaItemManager, BreakGridTask breakGridTask, EffectDatabase effectDatabase)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _explodeItemTask = new(_gridCellManager);
            _explodeItemTask.AddTo(ref builder);

            _breakGridTask = breakGridTask;
            _swapItemTask = new(_gridCellManager, _matchItemsTask);
            _inputProcessor = new(boardInput, _gridCellManager, _swapItemTask);
            _inputProcessor.AddTo(ref builder);

            _moveItemTask = new(_gridCellManager, _breakGridTask);
            _moveItemTask.AddTo(ref builder);

            _activateBoosterTask = new(_gridCellManager, _breakGridTask, effectDatabase, _explodeItemTask);
            _activateBoosterTask.AddTo(ref builder);

            _comboBoosterHandleTask = new(_gridCellManager, _breakGridTask, itemManager
                                , _activateBoosterTask, effectDatabase, _explodeItemTask);
            _comboBoosterHandleTask.AddTo(ref builder);

            _breakGridTask.SetActivateBoosterTask(_activateBoosterTask);
            _swapItemTask.SetComboBoosterHandler(_comboBoosterHandleTask);

            _spawnItemTask = spawnItemTask;
            _spawnItemTask.SetMoveItemTask(_moveItemTask);

            _detectMoveTask = new(_gridCellManager, _matchItemsTask);
            _detectMoveTask.AddTo(ref builder);

            _suggestTask = new(_gridCellManager, _detectMoveTask);
            _suggestTask.AddTo(ref builder);

            _checkGridTask = new(_gridCellManager, _moveItemTask, _spawnItemTask, _matchItemsTask);
            _checkGridTask.AddTo(ref builder);

            SetCheckGridTask();
            _disposable = builder.Build();
        }

        public void SetInputActive(bool isActive)
        {
            _inputProcessor.IsActive = isActive;
        }

        public void BuildSuggest()
        {
            _detectMoveTask.BuildLevelBoard();
        }

        public void Suggest(bool isSuggest)
        {
            _suggestTask.Suggest(isSuggest);
        }

        private void SetCheckGridTask()
        {
            _moveItemTask.SetCheckGridTask(_checkGridTask);
            _breakGridTask.SetCheckGridTask(_checkGridTask);
            _matchItemsTask.SetCheckGridTask(_checkGridTask);
            _spawnItemTask.SetCheckGridTask(_checkGridTask);
            _activateBoosterTask.SetCheckGridTask(_checkGridTask);
            _comboBoosterHandleTask.SetCheckGridTask(_checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
