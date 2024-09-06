using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckGameBoardMovementTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        private ReactiveProperty<bool> _aggregateValue;
        private Observer<ReactiveProperty<bool>> _lockObserver;

        private TimeSpan _gridLockThrottle;
        private IDisposable _reactivePropertyDisposable;
        private IDisposable _gridLockDisposable;

        public Observable<ReactiveProperty<bool>> LockObservable { get; private set; }

        public CheckGameBoardMovementTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;

            _gridLockThrottle = TimeSpan.FromSeconds(0.5f);
            DisposableBuilder builder = Disposable.CreateBuilder();

            _aggregateValue = new();
            _aggregateValue.AddTo(ref builder);

            _reactivePropertyDisposable = builder.Build();
        }

        public void BuildCheckBoard()
        {
            using(ListPool<ReactiveProperty<bool>>.Get(out List<ReactiveProperty<bool>> gridLockProperties))
            {
                using var positionPool = ListPool<Vector3Int>.Get(out var activePositions);
                activePositions.AddRange(_gridCellManager.GetActivePositions());

                foreach (Vector3Int position in activePositions)
                {
                    IGridCell gridCell = _gridCellManager.Get(position);
                    gridLockProperties.Add(gridCell.CheckLockProperty);
                }

                DisposableBuilder builder = Disposable.CreateBuilder();
                var gridLockObservable = gridLockProperties.ToObservable();
                var latestObservable = Observable.CombineLatest(gridLockObservable)
                                                 .Select(properties => properties.Aggregate((a, b) =>
                                                 {
                                                     _aggregateValue.Value = a.Value || b.Value;
                                                     return _aggregateValue;
                                                 }));

                var lockStates = latestObservable.Where(value => value.Value);
                var unlockStates = latestObservable.Debounce(_gridLockThrottle)
                                                   .Where(value => !value.Value);

                LockObservable = Observable.Merge(lockStates, unlockStates);
                LockObservable.Subscribe().AddTo(ref builder);
                
                _gridLockDisposable = builder.Build();
            }
        }

        public void Dispose()
        {
            _reactivePropertyDisposable.Dispose();
            _gridLockDisposable?.Dispose();
        }
    }
}
