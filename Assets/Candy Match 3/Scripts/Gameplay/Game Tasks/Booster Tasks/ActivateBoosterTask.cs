using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
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
        private readonly ISubscriber<ActivateBoosterMessage> _activateBoosterSubscriber;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private IDisposable _disposable;
        private IDisposable _messageDisposable;

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

            var messageBuilder = MessagePipe.DisposableBag.CreateBuilder();
            _activateBoosterSubscriber = GlobalMessagePipe.GetSubscriber<ActivateBoosterMessage>();
            _activateBoosterSubscriber.Subscribe(message => ActivateBooster(message).Forget())
                                      .AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            _cts = new();
            _token = _cts.Token;
        }

        public async UniTask ActivateBooster(ActivateBoosterMessage message)
        {
            IGridCell gridCell = _gridCellManager.Get(message.Position);

            if (message.Sender is IColorBooster colorBooster)
            {
                gridCell.LockStates = LockStates.Preparing;
                BoosterType boosterType = colorBooster.ColorBoosterType;

                switch (boosterType)
                {
                    case BoosterType.Horizontal:
                        await _horizontalBoosterTask.Activate(gridCell, false, false, null);
                        break;
                    case BoosterType.Vertical:
                        await _verticalBoosterTask.Activate(gridCell, false, false, null);
                        break;
                    case BoosterType.Wrapped:
                        await _wrappedBoosterTask.Activate(gridCell, 2, false, false, false, null);
                        break;
                }

                gridCell.LockStates = LockStates.None;
            }
        }

        public async UniTask ActivateBooster(IGridCell gridCell, bool useDelay, bool doNotCheck, bool isCreateBooster
            , int stage = 1, Action<BoundsInt> onActivate = null)
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
                ActiveBoosterCount = ActiveBoosterCount + 1;
                gridCell.LockStates = LockStates.Breaking;

                await booster.Activate();
                await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);

                if (blockItem is IColorBooster colorBooster)
                {
                    BoosterType colorBoosterType = colorBooster.ColorBoosterType;
                    switch (colorBoosterType)
                    {
                        case BoosterType.Horizontal:
                            await _horizontalBoosterTask.Activate(gridCell, useDelay, doNotCheck, onActivate);
                            break;
                        case BoosterType.Vertical:
                            await _verticalBoosterTask.Activate(gridCell, useDelay, doNotCheck, onActivate);
                            break;
                        case BoosterType.Wrapped:
                            await _wrappedBoosterTask.Activate(gridCell, stage, useDelay, doNotCheck, isCreateBooster, onActivate);
                            break;
                    }
                }

                else if (blockItem.ItemType == ItemType.ColorBomb)
                {
                    await _colorfulBoosterTask.Activate(position);
                }
            }

            gridCell.LockStates = LockStates.None;
            ActiveBoosterCount = ActiveBoosterCount - 1;
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
            _cts.Dispose();
            _messageDisposable.Dispose();
            _disposable.Dispose();
        }
    }
}
