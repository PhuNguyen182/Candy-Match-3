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

        private TimeSpan _gridLockThrottle;
        private IDisposable _gridLockDisposable;

        public bool IsBoardLock { get; private set; }
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
                    gridCell.CheckLockProperty
                            .Subscribe(SetLockValue)
                            .AddTo(ref builder);
                }

                Observable<bool> lockStates = LockProperty.Where(isGridLocked => isGridLocked);
                Observable<bool> unlockStates = LockProperty.Where(isGridLocked => !isGridLocked)
                                                            .Debounce(_gridLockThrottle);

                LockObservable = Observable.Merge(lockStates, unlockStates);
                LockObservable.Where(isLocked => isLocked).Take(1)
                              .Subscribe(value => SendBoardStopMessage(false))
                              .AddTo(ref builder);

                LockObservable.Subscribe(isBoardLock =>
                              {
                                  if (!isBoardLock)
                                      SendBoardStopMessage(true);
                              }).AddTo(ref builder);

                _gridLockDisposable = builder.Build();
            }
        }

        public void SendBoardStopMessage(bool isBoardLock)
        {
            _boardStopMessage.Publish(new BoardStopMessage
            {
                IsStopped = isBoardLock
            });
        }

        private void SetLockValue(bool isLocked)
        {
            IsBoardLock = isLocked;
            LockProperty.Value = isLocked;
        }

        public void Dispose()
        {
            _gridLockDisposable?.Dispose();
        }
    }
}
