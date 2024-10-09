using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class ComboBoosterHandleTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly ActivateBoosterTask _activateBoosterTask;

        private readonly DoubleStripedBoosterTask _doubleStripedBoosterTask;
        private readonly StripedWrappedBoosterTask _stripedWrappedBoosterTask;
        private readonly DoubleWrappedBoosterTask _doubleWrappedBoosterTask;
        private readonly ColorfulStripedBoosterTask _colorfulStripedBoosterTask;
        private readonly ColorfulWrappedBoosterTask _colorfulWrappedBoosterTask;
        private readonly DoubleColorfulBoosterTask _doubleColorfulBoosterTask;

        private CheckGridTask _checkGridTask;
        private IDisposable _disposable;

        public ColorfulStripedBoosterTask ColorfulStripedBoosterTask => _colorfulStripedBoosterTask;
        public ColorfulWrappedBoosterTask ColorfulWrappedBoosterTask => _colorfulWrappedBoosterTask;

        public ComboBoosterHandleTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ItemManager itemManager
            , ActivateBoosterTask activateBoosterTask, EffectDatabase effectDatabase, ExplodeItemTask explodeItemTask)
        {
            _itemManager = itemManager;
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _activateBoosterTask = activateBoosterTask;

            DisposableBuilder builder = Disposable.CreateBuilder();

            _doubleStripedBoosterTask = new(_gridCellManager, _breakGridTask);
            _doubleStripedBoosterTask.AddTo(ref builder);

            _stripedWrappedBoosterTask = new(_gridCellManager, _breakGridTask);
            _stripedWrappedBoosterTask.AddTo(ref builder);

            _doubleWrappedBoosterTask = new(_gridCellManager, _breakGridTask, explodeItemTask);
            _doubleWrappedBoosterTask.AddTo(ref builder);

            _colorfulStripedBoosterTask = new(_itemManager, _gridCellManager, _breakGridTask
                , _activateBoosterTask, effectDatabase.ColorfulFireray);
            _colorfulStripedBoosterTask.AddTo(ref builder);

            _colorfulWrappedBoosterTask = new(_itemManager, _gridCellManager, _breakGridTask
                , _activateBoosterTask, effectDatabase.ColorfulFireray);
            _colorfulWrappedBoosterTask.AddTo(ref builder);

            _doubleColorfulBoosterTask = new(_gridCellManager, _breakGridTask, effectDatabase.ColorfulFireray);
            _doubleColorfulBoosterTask.AddTo(ref builder);

            _disposable = builder.Build();
        }

        public async UniTask HandleComboBooster(IGridCell gridCell1, IGridCell gridCell2)
        {
            _checkGridTask.CanCheck = false;

            if (IsDoubleStripedCombo(gridCell1, gridCell2))
                await _doubleStripedBoosterTask.Activate(gridCell1, gridCell2);

            else if (IsStripedWrappedCombo(gridCell1, gridCell2))
                await _stripedWrappedBoosterTask.Activate(gridCell1, gridCell2);

            else if (IsDoubleWrappedCombo(gridCell1, gridCell2))
                await _doubleWrappedBoosterTask.Activate(gridCell1, gridCell2);

            else if (IsColorfulStripedCombo(gridCell1, gridCell2))
                await _colorfulStripedBoosterTask.Activate(gridCell1, gridCell2);

            else if (IsColorfulWrappedCombo(gridCell1, gridCell2))
                await _colorfulWrappedBoosterTask.Activate(gridCell1, gridCell2);

            else if (IsDoubleColorfulCombo(gridCell1, gridCell2))
                await _doubleColorfulBoosterTask.Activate(gridCell1, gridCell2);
            
            _checkGridTask.CanCheck = true;
        }

        public async UniTask CombineColorfulItemWithColorItem(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            if(blockItem1.ItemType == ItemType.ColorBomb)
            {
                CandyColor candyColor = blockItem2.CandyColor;
                await _activateBoosterTask.ColorfulBoosterTask.ActivateWithColor(gridCell1, candyColor);
            }

            else if(blockItem2.ItemType == ItemType.ColorBomb)
            {
                CandyColor candyColor = blockItem1.CandyColor;
                await _activateBoosterTask.ColorfulBoosterTask.ActivateWithColor(gridCell2, candyColor);
            }
        }

        private bool IsDoubleStripedCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return IsStripedBooster(blockItem1) && IsStripedBooster(blockItem2);
        }

        private bool IsStripedWrappedCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return (IsStripedBooster(blockItem1) && IsWrappedBooster(blockItem2))
                || (IsStripedBooster(blockItem2) && IsWrappedBooster(blockItem1));
        }

        private bool IsDoubleWrappedCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return IsWrappedBooster(blockItem1) && IsWrappedBooster(blockItem2);
        }

        private bool IsColorfulStripedCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return (IsStripedBooster(blockItem1) && blockItem2.ItemType == ItemType.ColorBomb)
                || (IsStripedBooster(blockItem2) && blockItem1.ItemType == ItemType.ColorBomb);
        }

        private bool IsColorfulWrappedCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return (IsWrappedBooster(blockItem1) && blockItem2.ItemType == ItemType.ColorBomb)
                || (IsWrappedBooster(blockItem2) && blockItem1.ItemType == ItemType.ColorBomb);
        }

        private bool IsDoubleColorfulCombo(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return blockItem1.ItemType == ItemType.ColorBomb && blockItem2.ItemType == ItemType.ColorBomb;
        }

        private bool IsStripedBooster(IBlockItem blockItem)
        {
            if (blockItem is IColorBooster colorBooster)
                return colorBooster.ColorBoosterType == BoosterType.Horizontal
                    || colorBooster.ColorBoosterType == BoosterType.Vertical;

            return false;
        }

        private bool IsWrappedBooster(IBlockItem blockItem)
        {
            if (blockItem is IColorBooster colorBooster)
                return colorBooster.ColorBoosterType == BoosterType.Wrapped;

            return false;
        }

        public bool IsComboBooster(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            if (blockItem1 is IBooster && blockItem2 is IBooster)
                return true;

            return false;
        }

        public bool IsSwapToColorful(IGridCell gridCell1, IGridCell gridCell2)
        {
            IBlockItem blockItem1 = gridCell1.BlockItem;
            IBlockItem blockItem2 = gridCell2.BlockItem;

            return (blockItem1.ItemType == ItemType.ColorBomb && blockItem2.CandyColor != CandyColor.None)
                || (blockItem2.ItemType == ItemType.ColorBomb && blockItem1.CandyColor != CandyColor.None);
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
            _doubleStripedBoosterTask.SetCheckGridTask(_checkGridTask);
            _stripedWrappedBoosterTask.SetCheckGridTask(_checkGridTask);
            _doubleWrappedBoosterTask.SetCheckGridTask(_checkGridTask);
            _doubleColorfulBoosterTask.SetCheckGridTask(_checkGridTask);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
