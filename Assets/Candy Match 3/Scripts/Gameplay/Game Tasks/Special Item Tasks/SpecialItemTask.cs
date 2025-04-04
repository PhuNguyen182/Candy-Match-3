using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.SpecialItemTasks
{
    public class SpecialItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        private IDisposable _disposable;

        public ExpandableItemTask ExpandableItemTask { get; }

        public SpecialItemTask(GridCellManager gridCellManager, ItemManager itemManager, BreakGridTask breakGridTask)
        {
            _gridCellManager = gridCellManager;
            var builder = Disposable.CreateBuilder();
            
            ExpandableItemTask = new(_gridCellManager, itemManager, breakGridTask);
            ExpandableItemTask.AddTo(ref builder);

            _disposable = builder.Build();
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            ExpandableItemTask.SetCheckGridTask(checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
