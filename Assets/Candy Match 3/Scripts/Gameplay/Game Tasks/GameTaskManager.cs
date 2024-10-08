using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Gameplay.GameTasks.SpecialItemTasks;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        #region Most Important Tasks
        private readonly InputProcessTask _inputProcessor;
        private readonly GridCellManager _gridCellManager;
        #endregion

        #region Basic Tasks
        private readonly MoveItemTask _moveItemTask;
        private readonly SwapItemTask _swapItemTask;
        private readonly SpawnItemTask _spawnItemTask;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MatchItemsTask _matchItemsTask;
        #endregion

        #region Booster And Special Item Tasks
        private readonly ExplodeItemTask _explodeItemTask;
        private readonly SpecialItemTask _specialItemTask;
        private readonly ActivateBoosterTask _activateBoosterTask;
        private readonly ComboBoosterHandleTask _comboBoosterHandleTask;
        private readonly InGameBoosterTasks _inGameBoosterTasks;
        #endregion

        #region Advanced Tasks
        private readonly SuggestTask _suggestTask;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly FindItemRegionTask _findItemRegionTask;
        private readonly MatchRegionTask _matchRegionTask;
        private readonly ShuffleBoardTask _shuffleBoardTask;
        #endregion

        #region Game State Tasks
        private readonly StartGameTask _startGameTask;
        private readonly GameStateController _gameStateController;
        private readonly CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private readonly CheckTargetTask _checkTargetTask;
        private readonly EndGameTask _endGameTask;
        #endregion

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager, ItemManager itemManager, SpawnItemTask spawnItemTask
            , MatchItemsTask matchItemsTask, MetaItemManager metaItemManager, BreakGridTask breakGridTask, MainGamePanel mainGamePanel
            , EndGameScreen endGameScreen, InGameSettingPanel settingSidePanel, InGameBoosterPanel inGameBoosterPanel, SpecialItemTask specialItemTask
            , ComplimentTask complimentTask, FillBoardTask fillBoardTask, ScriptableDatabaseCollection databaseCollection)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;
            _matchItemsTask = matchItemsTask;

            _explodeItemTask = new(_gridCellManager, databaseCollection.ExplodeEffectCollection);
            _explodeItemTask.AddTo(ref builder);

            _detectMoveTask = new(_gridCellManager, _matchItemsTask);
            _detectMoveTask.AddTo(ref builder);

            _suggestTask = new(_gridCellManager, _detectMoveTask, matchItemsTask);
            _suggestTask.AddTo(ref builder);

            _startGameTask = new();
            _startGameTask.AddTo(ref builder);

            _breakGridTask = breakGridTask;
            _swapItemTask = new(_gridCellManager, _matchItemsTask, _suggestTask, _breakGridTask, databaseCollection.EffectDatabase);
            _swapItemTask.AddTo(ref builder);

            _inputProcessor = new(boardInput, _gridCellManager, _swapItemTask);
            _inputProcessor.AddTo(ref builder);

            _moveItemTask = new(_gridCellManager, _breakGridTask);
            _moveItemTask.AddTo(ref builder);

            _activateBoosterTask = new(_gridCellManager, _breakGridTask, databaseCollection.EffectDatabase, _explodeItemTask, _suggestTask);
            _activateBoosterTask.AddTo(ref builder);

            _comboBoosterHandleTask = new(_gridCellManager, _breakGridTask, itemManager, _activateBoosterTask
                                         , databaseCollection.EffectDatabase, _explodeItemTask, _suggestTask);
            _comboBoosterHandleTask.AddTo(ref builder);

            _breakGridTask.SetActivateBoosterTask(_activateBoosterTask);
            _swapItemTask.SetComboBoosterHandler(_comboBoosterHandleTask);

            _inGameBoosterTasks = new(_inputProcessor, _gridCellManager, _breakGridTask, _suggestTask, _explodeItemTask, _swapItemTask
                                    , settingSidePanel, _activateBoosterTask, _comboBoosterHandleTask, itemManager, inGameBoosterPanel
                                    , databaseCollection.InGameBoosterPackDatabase, databaseCollection.EffectDatabase);
            _inGameBoosterTasks.AddTo(ref builder);
            _inputProcessor.SetInGameBoosterTasks(_inGameBoosterTasks);

            _spawnItemTask = spawnItemTask;
            _spawnItemTask.SetMoveItemTask(_moveItemTask);

            _findItemRegionTask = new(_gridCellManager);
            _findItemRegionTask.AddTo(ref builder);

            _checkGameBoardMovementTask = new(_gridCellManager);
            _checkGameBoardMovementTask.AddTo(ref builder);
            complimentTask.SetCheckGameBoardMovementTask(_checkGameBoardMovementTask);
            _inGameBoosterTasks.SetCheckBoardMovementTask(_checkGameBoardMovementTask);

            _matchRegionTask = new(_gridCellManager, _findItemRegionTask, _matchItemsTask, _suggestTask);
            _matchRegionTask.AddTo(ref builder);

            _shuffleBoardTask = new(_gridCellManager, _inputProcessor, _detectMoveTask, _suggestTask, fillBoardTask);
            _shuffleBoardTask.AddTo(ref builder);

            _checkTargetTask = new(_matchRegionTask, databaseCollection.TargetDatabase, _shuffleBoardTask, mainGamePanel);
            _suggestTask.SetCheckGameBoardMovementTask(_checkGameBoardMovementTask);
            _inputProcessor.SetCheckGameBoardMovementTask(_checkGameBoardMovementTask);

            _endGameTask = new(_checkTargetTask, _checkGameBoardMovementTask, _activateBoosterTask, endGameScreen);
            _endGameTask.AddTo(ref builder);

            _checkGridTask = new(_gridCellManager, _moveItemTask, _spawnItemTask, _matchRegionTask);
            _checkGridTask.AddTo(ref builder);

            _gameStateController = new(_inputProcessor, _checkTargetTask, _startGameTask, complimentTask, _endGameTask
                                        , endGameScreen, _suggestTask, settingSidePanel);
            _gameStateController.AddTo(ref builder);
            _specialItemTask = specialItemTask;

            SetCheckGridTask();
            _disposable = builder.Build();
        }

        public void StartGame(LevelModel levelModel)
        {
            _gameStateController.SetLevelModel(levelModel);
            _gameStateController.StartGame();
        }

        public void InitInGameBooster()
        {
            _inGameBoosterTasks.InitStartBooster();
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

        public void BuildShufflePositions()
        {
            _shuffleBoardTask.BuildActivePositions();
        }

        public void ShuffleBoard()
        {
            _shuffleBoardTask.Shuffle();
        }

        private void SetCheckGridTask()
        {
            _moveItemTask.SetCheckGridTask(_checkGridTask);
            _breakGridTask.SetCheckGridTask(_checkGridTask);
            _matchItemsTask.SetCheckGridTask(_checkGridTask);
            _matchRegionTask.SetCheckGridTask(_checkGridTask);
            _spawnItemTask.SetCheckGridTask(_checkGridTask);
            _swapItemTask.SetCheckGridTask(_checkGridTask);

            _activateBoosterTask.SetCheckGridTask(_checkGridTask);
            _comboBoosterHandleTask.SetCheckGridTask(_checkGridTask);
            _inGameBoosterTasks.SetCheckGridTask(_checkGridTask);
            _specialItemTask.SetCheckGridTask(_checkGridTask);

            _endGameTask.SetCheckGridTask(_checkGridTask);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _disposable.Dispose();
        }
    }
}
