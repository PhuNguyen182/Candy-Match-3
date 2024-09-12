using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class PlaceBoosterTask : IDisposable
    {
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;
        private readonly ActivateBoosterTask _activateBoosterTask;
        private readonly ColorfulStripedBoosterTask _colorfulStripedBoosterTask;
        private readonly ColorfulWrappedBoosterTask _colorfulWrappedBoosterTask;
        private readonly ItemManager _itemManager;

        private readonly IPublisher<UseInGameBoosterMessage> _useInGameBoosterPublisher;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public PlaceBoosterTask(GridCellManager gridCellManager, BreakGridTask breakGridTask, ActivateBoosterTask activateBoosterTask
            , ItemManager itemManager, ColorfulStripedBoosterTask colorfulStripedBoosterTask, ColorfulWrappedBoosterTask colorfulWrappedBoosterTask)
        {
            _breakGridTask = breakGridTask;
            _gridCellManager = gridCellManager;
            _activateBoosterTask = activateBoosterTask;
            _itemManager = itemManager;

            _cts = new();
            _token = _cts.Token;
            _useInGameBoosterPublisher = GlobalMessagePipe.GetPublisher<UseInGameBoosterMessage>();
        }

        public async UniTask Activate(Vector3Int position)
        {
            IGridCell gridCell = _gridCellManager.Get(position);

            if (gridCell == null)
                return;

            if (!gridCell.HasItem || gridCell.IsLocked)
                return;

            IBlockItem blockItem = gridCell.BlockItem;
            if (!blockItem.IsMatchable)
                return;

            IColorBooster colorBooster = null;
            gridCell.LockStates = LockStates.Replacing;
            CandyColor candyColor = blockItem.CandyColor;

            _useInGameBoosterPublisher.Publish(new UseInGameBoosterMessage
            {
                BoosterType = InGameBoosterType.Colorful
            });

            if (blockItem is IColorBooster booster)
                colorBooster = booster;         

            blockItem.ItemBlast().Forget();
            _breakGridTask.ReleaseGridCell(gridCell);

            _itemManager.Add(new BlockItemPosition
            {
                Position = gridCell.GridPosition,
                ItemData = new BlockItemData
                {
                    ID = 0,
                    HealthPoint = 1,
                    ItemType = ItemType.ColorBomb,
                    ItemColor = CandyColor.None,
                }
            });

            gridCell.LockStates = LockStates.None;
            TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, _token);

            if(colorBooster != null)
            {
                if (colorBooster.ColorBoosterType == BoosterType.Wrapped)
                    await _colorfulWrappedBoosterTask.Activate(gridCell, blockItem);

                else
                    await _colorfulStripedBoosterTask.Activate(gridCell, blockItem);
            }

            else
                await _activateBoosterTask.ColorfulBoosterTask.ActivateWithColor(gridCell, candyColor);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
