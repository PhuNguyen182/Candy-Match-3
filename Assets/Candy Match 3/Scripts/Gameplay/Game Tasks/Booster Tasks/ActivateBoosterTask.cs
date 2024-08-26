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

        public ColorfulBoosterTask ColorfulBoosterTask => _colorfulBoosterTask;

        public ActivateBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, EffectDatabase effectDatabase)
        {
            _breakGridTask = breakGridTask;
            _colorfulBoosterTask = new(gridCellManager, breakGridTask, effectDatabase.ColorfulFireray);
            _horizontalBoosterTask = new(gridCellManager, breakGridTask);
            _verticalBoosterTask = new(gridCellManager, breakGridTask);
            _wrappedBoosterTask = new(gridCellManager, breakGridTask);
        }

        public async UniTask ActivateBooster(IGridCell gridCell)
        {
            Vector3Int position = gridCell.GridPosition;
            
            if (!gridCell.HasItem)
                return;
            
            IBlockItem blockItem = gridCell.BlockItem;
            if (blockItem is IBooster booster)
            {
                if (booster.IsActivated)
                    return;

                booster.IsActivated = true;

                if (blockItem is ISetColorBooster colorBooster)
                {
                    ColorBoosterType colorBoosterType = colorBooster.ColorBoosterType;
                    switch (colorBoosterType)
                    {
                        case ColorBoosterType.Horizontal:
                            await _horizontalBoosterTask.Activate(gridCell);
                            break;
                        case ColorBoosterType.Vertical:
                            await _verticalBoosterTask.Activate(gridCell);
                            break;
                        case ColorBoosterType.Wrapped:
                            await _wrappedBoosterTask.Activate(gridCell);
                            break;
                    }
                }

                else if (blockItem.ItemType == ItemType.ColorBomb)
                {
                    await _colorfulBoosterTask.Activate(position);
                }
            }
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
            
        }
    }
}
