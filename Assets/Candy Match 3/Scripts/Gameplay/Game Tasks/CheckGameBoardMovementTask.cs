using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Constants;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGameBoardMovementTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly IPublisher<BoardStopMessage> _boardStopMessage;

        private int _gridLockedCount = 0;
        private TimeSpan _gridLockThrottle;
        private IDisposable _disposable;

        public bool AllGridsUnlocked { get; private set; }
        public ReactiveProperty<bool> UnlockedProperty { get; private set; }

        public CheckGameBoardMovementTask(GridCellManager gridCellManager)
        {
            UnlockedProperty = new();
            _gridCellManager = gridCellManager;
            _gridLockThrottle = TimeSpan.FromSeconds(Match3Constants.RegionMatchDelay);
            _boardStopMessage = GlobalMessagePipe.GetPublisher<BoardStopMessage>();
        }

        public void BuildCheckBoard()
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            foreach (IGridCell gridCell in _gridCellManager.GridCells)
                gridCell.GridLockProperty.Subscribe(SetLockValue).AddTo(ref builder);

            UnlockedProperty.Debounce(_gridLockThrottle).Subscribe(SendBoardStopMessage).AddTo(ref builder);
            _disposable = builder.Build();
        }

        public void SendBoardStopMessage(bool isUnlocked)
        {
            AllGridsUnlocked = isUnlocked;
            if (isUnlocked) Debug.Log("Stopped!");
            _boardStopMessage.Publish(new BoardStopMessage
            {
                IsStopped = isUnlocked
            });
        }

        private void SetLockValue(bool isLocked)
        {
            int step = isLocked ? -1 : 1;
            _gridLockedCount = _gridLockedCount + step;
            UnlockedProperty.Value = IsAllGridsUnlocked();
        }

        private bool IsAllGridsUnlocked()
        {
            return _gridCellManager.PositionCount == _gridLockedCount;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
