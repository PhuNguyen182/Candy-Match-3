using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Databases;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class ActivateBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly ColorfulBoosterTask _colorfulBoosterTask;
        private readonly HorizontalStripedBoosterTask _horizontalBoosterTask;
        private readonly VerticalStripedBoosterTask _verticalBoosterTask;
        private readonly WrappedBoosterTask _wrappedBoosterTask;

        private IDisposable _disposable;

        public ColorfulBoosterTask ColorfulBoosterTask => _colorfulBoosterTask;

        public ActivateBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask
            , EffectDatabase effectDatabase, ExplodeItemTask explodeItemTask)
        {
            _breakGridTask = breakGridTask;
            DisposableBuilder builder = Disposable.CreateBuilder();

            _colorfulBoosterTask = new(gridCellManager, breakGridTask, effectDatabase.ColorfulFireray);
            _colorfulBoosterTask.AddTo(ref builder);
            
            _horizontalBoosterTask = new(gridCellManager, breakGridTask);
            _horizontalBoosterTask.AddTo(ref builder);
            
            _verticalBoosterTask = new(gridCellManager, breakGridTask);
            _verticalBoosterTask.AddTo(ref builder);

            _wrappedBoosterTask = new(gridCellManager, breakGridTask, explodeItemTask);
            _wrappedBoosterTask.AddTo(ref builder);
            
            _disposable = builder.Build();
        }

        public async UniTask ActivateBooster(IGridCell gridCell, bool useDelay, bool doNotCheck)
        {
            Vector3Int position = gridCell.GridPosition;
            
            if (!gridCell.HasItem)
                return;

            gridCell.LockStates = LockStates.Breaking;
            IBlockItem blockItem = gridCell.BlockItem;

            if (blockItem is IBooster booster)
            {
                if (booster.IsActivated)
                    return;

                booster.IsActivated = true;
                await booster.Activate();

                if (blockItem is IColorBooster colorBooster)
                {
                    ColorBoosterType colorBoosterType = colorBooster.ColorBoosterType;
                    switch (colorBoosterType)
                    {
                        case ColorBoosterType.Horizontal:
                            await _horizontalBoosterTask.Activate(gridCell, useDelay, doNotCheck);
                            break;
                        case ColorBoosterType.Vertical:
                            await _verticalBoosterTask.Activate(gridCell, useDelay, doNotCheck);
                            break;
                        case ColorBoosterType.Wrapped:
                            await _wrappedBoosterTask.Activate(gridCell, useDelay, doNotCheck);
                            break;
                    }
                }

                else if (blockItem.ItemType == ItemType.ColorBomb)
                {
                    await _colorfulBoosterTask.Activate(position);
                }
            }

            gridCell.LockStates = LockStates.None;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _colorfulBoosterTask.SetCheckGridTask(checkGridTask);
            _horizontalBoosterTask.SetCheckGridTask(checkGridTask);
            _verticalBoosterTask.SetCheckGridTask(checkGridTask);
            _wrappedBoosterTask.SetCheckGridTask(checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
