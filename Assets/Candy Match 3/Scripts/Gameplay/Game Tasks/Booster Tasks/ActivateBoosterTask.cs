using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
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
        private readonly SuggestTask _suggestTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private IDisposable _disposable;

        public int ActiveBoosterCount { get; private set; }
        public ColorfulBoosterTask ColorfulBoosterTask => _colorfulBoosterTask;

        public ActivateBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask
            , EffectDatabase effectDatabase, ExplodeItemTask explodeItemTask, SuggestTask suggestTask)
        {
            _breakGridTask = breakGridTask;
            _gridCellManager = gridCellManager;
            _suggestTask = suggestTask;
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

        public async UniTask ActivateBooster(IGridCell gridCell, bool useDelay, bool doNotCheck
            , bool isCreateBooster, Action<BoundsInt> onActivate = null)
        {
            Vector3Int position = gridCell.GridPosition;
            IBlockItem blockItem = gridCell.BlockItem;
            IBooster booster = blockItem as IBooster;

            if (booster.IsActivated)
                return;

            _suggestTask.Suggest(false);
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
                        await _wrappedBoosterTask.Activate(gridCell, useDelay, doNotCheck, isCreateBooster, onActivate);
                        break;
                }
            }

            else if (blockItem.ItemType == ItemType.ColorBomb)
            {
                await _colorfulBoosterTask.Activate(position);
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
            _disposable.Dispose();
        }
    }
}
