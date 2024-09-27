using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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
        public Observable<bool> LockObservable { get; private set; }
        public ReactiveProperty<bool> LockProperty { get; private set; }

        public CheckGameBoardMovementTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
            _gridLockThrottle = TimeSpan.FromSeconds(Match3Constants.RegionMatchDelay);
            _boardStopMessage = GlobalMessagePipe.GetPublisher<BoardStopMessage>();
            LockProperty = new();
        }

        public void BuildCheckBoard()
        {
            using (ListPool<Vector3Int>.Get(out List<Vector3Int> activePositions))
            {
                DisposableBuilder builder = Disposable.CreateBuilder();
                activePositions.AddRange(_gridCellManager.GetActivePositions());

                foreach (Vector3Int position in activePositions)
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    gridCell.CheckLockProperty.Subscribe(SetLockValue).AddTo(ref builder);
                }

                Observable<bool> lockedStates = LockProperty.Where(isGridLocked => isGridLocked);
                Observable<bool> unlockedStates = LockProperty.Where(isGridLocked => !isGridLocked)
                                                              .Debounce(_gridLockThrottle);

                LockObservable = Observable.Merge(lockedStates, unlockedStates);
                LockObservable.Where(isGridLocked => isGridLocked)
                              .Subscribe(_ => SendBoardStopMessage(false))
                              .AddTo(ref builder);

                LockObservable.Subscribe(isGridLocked =>
                              {
                                  if (!isGridLocked)
                                      SendBoardStopMessage(true);
                              }).AddTo(ref builder);

                _disposable = builder.Build();
            }
        }

        public void SendBoardStopMessage(bool isStopped)
        {
            if (isStopped)
            {
                bool isBoardStop = _gridCellManager.PositionCount == _gridLockedCount;

                if (isBoardStop)
                {
                    AllGridsUnlocked = true;
                    _boardStopMessage.Publish(new BoardStopMessage
                    {
                        IsStopped = true
                    });
                }

                else AllGridsUnlocked = false;
            }

            else
            {
                AllGridsUnlocked = false;
                _boardStopMessage.Publish(new BoardStopMessage
                {
                    IsStopped = false
                });
            }
        }

        private void SetLockValue(bool isLocked)
        {
            int step = isLocked ? -1 : 1;
            LockProperty.Value = isLocked;
            _gridLockedCount = _gridLockedCount + step;
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
