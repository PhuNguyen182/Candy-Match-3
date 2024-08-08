using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.Strategies;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.CustomData;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameTaskManager : IDisposable
    {
        private readonly InputProcessor _inputProcessor;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly CheckGridTask _checkGridTask;
        private readonly MoveItemTask _moveItemTask;
        private readonly SpawnItemTask _spawnItemTask;

        private IDisposable _disposable;

        public GameTaskManager(BoardInput boardInput, GridCellManager gridCellManager, ItemManager itemManager)
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = gridCellManager;

            _inputProcessor = new(boardInput, _gridCellManager);
            _inputProcessor.AddTo(ref builder);

            _breakGridTask = new(_gridCellManager);
            _moveItemTask = new(_gridCellManager);
            
            _checkGridTask = new(_gridCellManager, _moveItemTask);
            _checkGridTask.AddTo(ref builder);

            _moveItemTask.SetCheckGridTask(_checkGridTask);
            
            _spawnItemTask = new(_gridCellManager, itemManager);
            _spawnItemTask.AddTo(ref builder);

            _disposable = builder.Build();
        }

        public void CheckMoveOnStart()
        {
            _moveItemTask.MoveItems().Forget();
        }

        public void SetSpawnRules(List<SpawnRuleBlockData> spawnRuleData)
        {
            _spawnItemTask.SetItemSpawnerData(spawnRuleData);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
