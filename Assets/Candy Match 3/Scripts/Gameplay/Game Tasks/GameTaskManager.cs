using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
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
        private readonly InGameBoosterTasks _inGameBoosterTasks;
        private readonly CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private readonly GameStateController _gameStateController;
        private readonly CheckTargetTask _checkTargetTask;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly EndGameTask _endGameTask;
        private readonly SuggestTask _suggestTask;

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager, ItemManager itemManager, SpawnItemTask spawnItemTask
            , MatchItemsTask matchItemsTask, MetaItemManager metaItemManager, BreakGridTask breakGridTask, EffectDatabase effectDatabase
            , MainGamePanel mainGamePanel, EndGameScreen endGameScreen, TargetDatabase targetDatabase, InGameBoosterPanel inGameBoosterPanel
            , InGameBoosterPackDatabase inGameBoosterPackDatabase)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _explodeItemTask = new(_gridCellManager);
            _explodeItemTask.AddTo(ref builder);

            _detectMoveTask = new(_gridCellManager, _matchItemsTask);
            _detectMoveTask.AddTo(ref builder);

            _suggestTask = new(_gridCellManager, _detectMoveTask);
            _suggestTask.AddTo(ref builder);

            _breakGridTask = breakGridTask;
            _swapItemTask = new(_gridCellManager, _matchItemsTask, _suggestTask, _breakGridTask);
            _inputProcessor = new(boardInput, _gridCellManager, _swapItemTask);
            _inputProcessor.AddTo(ref builder);
            _suggestTask.SetInputProcessTask(_inputProcessor);

            _moveItemTask = new(_gridCellManager, _breakGridTask);
            _moveItemTask.AddTo(ref builder);

            _activateBoosterTask = new(_gridCellManager, _breakGridTask, effectDatabase, _explodeItemTask);
            _activateBoosterTask.AddTo(ref builder);

            _comboBoosterHandleTask = new(_gridCellManager, _breakGridTask, itemManager
                                , _activateBoosterTask, effectDatabase, _explodeItemTask);
            _comboBoosterHandleTask.AddTo(ref builder);

            _breakGridTask.SetActivateBoosterTask(_activateBoosterTask);
            _swapItemTask.SetComboBoosterHandler(_comboBoosterHandleTask);

            _inGameBoosterTasks = new(_inputProcessor, _gridCellManager, _breakGridTask, _suggestTask
                                , _swapItemTask, _activateBoosterTask, itemManager, inGameBoosterPanel, inGameBoosterPackDatabase);
            _inGameBoosterTasks.AddTo(ref builder);
            _inputProcessor.SetInGameBoosterTasks(_inGameBoosterTasks);

            _spawnItemTask = spawnItemTask;
            _spawnItemTask.SetMoveItemTask(_moveItemTask);

            _checkTargetTask = new(targetDatabase, mainGamePanel);
            _checkGameBoardMovementTask = new(_gridCellManager);
            _checkGameBoardMovementTask.AddTo(ref builder);
            _inGameBoosterTasks.SetCheckBoardMovementTask(_checkGameBoardMovementTask);

            _suggestTask.SetCheckGameBoardMovementTask(_checkGameBoardMovementTask);
            _inputProcessor.SetCheckGameBoardMovementTask(_checkGameBoardMovementTask);

            _endGameTask = new(_checkTargetTask, _checkGameBoardMovementTask, _activateBoosterTask);
            _endGameTask.AddTo(ref builder);

            _checkGridTask = new(_gridCellManager, _moveItemTask, _inputProcessor, _spawnItemTask, _matchItemsTask);
            _checkGridTask.AddTo(ref builder);

            _gameStateController = new(_inputProcessor, _checkTargetTask, _endGameTask, endGameScreen, _suggestTask);
            _gameStateController.AddTo(ref builder);

            SetCheckGridTask();
            _disposable = builder.Build();
        }

        public void InitInGameBooster()
        {
            _inGameBoosterTasks.InitBoosters(new()
            {
                new() { Amount = 100, BoosterType = InGameBoosterType.Break },
                new() { Amount = 100, BoosterType = InGameBoosterType.Blast },
                new() { Amount = 100, BoosterType = InGameBoosterType.Swap },
                new() { Amount = 100, BoosterType = InGameBoosterType.Colorful }
            });
        }

        public void SetInputActive(bool isActive)
        {
            _inputProcessor.IsActive = isActive;
        }

        public void BuildBoardMovementCheck()
        {
            _checkGameBoardMovementTask.BuildCheckBoard();
        }

        public void BuildSuggest()
        {
            _detectMoveTask.BuildLevelBoard();
        }

        public void BuildTarget(LevelModel levelModel)
        {
            _checkTargetTask.InitLevelTarget(levelModel);
        }

        public void Test(out bool isLock)
        {
            isLock = _checkGameBoardMovementTask.IsBoardLock;
            if (isLock)
                Debug.Log("Lock");
        }

        private void SetCheckGridTask()
        {
            _moveItemTask.SetCheckGridTask(_checkGridTask);
            _breakGridTask.SetCheckGridTask(_checkGridTask);
            _matchItemsTask.SetCheckGridTask(_checkGridTask);
            _spawnItemTask.SetCheckGridTask(_checkGridTask);
            _activateBoosterTask.SetCheckGridTask(_checkGridTask);
            _comboBoosterHandleTask.SetCheckGridTask(_checkGridTask);
            _swapItemTask.SetCheckGridTask(_checkGridTask);
            _inGameBoosterTasks.SetCheckGridTask(_checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
