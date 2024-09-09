using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class EndGameTask : IDisposable
    {
        private readonly CheckTargetTask _checkTargetTask;
        private readonly CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private readonly ActivateBoosterTask _activateBoosterTask;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private IDisposable _disposable;

        public EndGameTask(CheckTargetTask checkTargetTask, CheckGameBoardMovementTask checkGameBoardMovementTask, ActivateBoosterTask activateBoosterTask)
        {
            _checkTargetTask = checkTargetTask;
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
            _activateBoosterTask = activateBoosterTask;

            _cts = new();
            _token = _cts.Token;

            var builder = Disposable.CreateBuilder();
            _checkGameBoardMovementTask.CheckBoardLockProperty
                                       .Subscribe(x => Debug.Log(x))
                                       .AddTo(ref builder);
            _disposable = builder.Build();

            _checkTargetTask.SetEndGameTask(this);
        }

        public async UniTask OnWinGame()
        {
            await WaitForBoardStop();
        }

        public async UniTask OnLoseGame()
        {
            await WaitForBoardStop();
        }

        public async UniTask WaitForBoardStop()
        {
            if (_checkGameBoardMovementTask.LockObservable != null)
            {
                await _checkGameBoardMovementTask.LockObservable.Where(value => !value.Value).Take(1).WaitAsync();
                await UniTask.WaitUntil(() => _activateBoosterTask.ActiveBoosterCount <= 0 && _checkTargetTask.IsAllItemsStop()
                , PlayerLoopTiming.Update, _token);
            }
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
