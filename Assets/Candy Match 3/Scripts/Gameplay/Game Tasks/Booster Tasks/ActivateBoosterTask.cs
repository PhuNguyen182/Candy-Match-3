using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using CandyMatch3.Scripts.Common.Databases;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class ActivateBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly ColorfulBoosterTask _colorfulBoosterTask;
        private readonly HorizontalStripedBoosterTask _horizontalBoosterTask;
        private readonly VerticalStripedBoosterTask _verticalBoosterTask;
        private readonly WrappedBoosterTask _wrappedBoosterTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;
        private IDisposable _disposable;

        public int ActiveBoosterCount { get; private set; }
        public ColorfulBoosterTask ColorfulBoosterTask => _colorfulBoosterTask;

        public ActivateBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask
            , EffectDatabase effectDatabase, ExplodeItemTask explodeItemTask)
        {
            _breakGridTask = breakGridTask;
            _gridCellManager = gridCellManager;
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

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask ActivateBooster(IGridCell gridCell, bool useDelay, bool doNotCheck, Action<BoundsInt> onActivate = null)
        {
            Vector3Int position = gridCell.GridPosition;
            IBlockItem blockItem = gridCell.BlockItem;
            IBooster booster = blockItem as IBooster;

            if (booster.IsActivated)
                return;

            booster.IsActivated = true;
            UniTask boosterTask = UniTask.CompletedTask;

            if (gridCell.GridStateful is IBreakable stateBreakable)
            {
                // Break stateful first if available
                bool isLockedState = gridCell.GridStateful.IsLocked;

                if (stateBreakable.Break())
                {
                    _checkGridTask.CheckMatchAtPosition(position);
                    gridCell.SetGridStateful(new AvailableState());
                }
            }

            ActiveBoosterCount = ActiveBoosterCount + 1;
            gridCell.LockStates = LockStates.Breaking;

            await booster.Activate();
            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);

            if (blockItem is IColorBooster colorBooster)
            {
                BoosterType colorBoosterType = colorBooster.ColorBoosterType;
                boosterTask = colorBoosterType switch
                {
                    BoosterType.Horizontal => _horizontalBoosterTask.Activate(gridCell, useDelay, doNotCheck, onActivate),
                    BoosterType.Vertical => _verticalBoosterTask.Activate(gridCell, useDelay, doNotCheck, onActivate),
                    BoosterType.Wrapped => _wrappedBoosterTask.Activate(gridCell, useDelay, doNotCheck, onActivate),
                    _ => UniTask.CompletedTask
                };

                await boosterTask.ContinueWith(() => ActiveBoosterCount = ActiveBoosterCount - 1);
            }

            else if (blockItem.ItemType == ItemType.ColorBomb)
            {
                boosterTask = _colorfulBoosterTask.Activate(position);
                await boosterTask.ContinueWith(() => ActiveBoosterCount = ActiveBoosterCount - 1);
            }

            gridCell.LockStates = LockStates.None;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
            _colorfulBoosterTask.SetCheckGridTask(checkGridTask);
            _horizontalBoosterTask.SetCheckGridTask(checkGridTask);
            _verticalBoosterTask.SetCheckGridTask(checkGridTask);
            _wrappedBoosterTask.SetCheckGridTask(checkGridTask);
        }

        public void Dispose()
        {
            _cts.Dispose();
            _disposable.Dispose();
        }
    }
}
