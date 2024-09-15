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
        private readonly TimeSpan _waitTimeAmount;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private IDisposable _disposable;

        public EndGameTask(CheckTargetTask checkTargetTask, CheckGameBoardMovementTask checkGameBoardMovementTask, ActivateBoosterTask activateBoosterTask)
        {
            _checkTargetTask = checkTargetTask;
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
            _activateBoosterTask = activateBoosterTask;
            _waitTimeAmount = TimeSpan.FromSeconds(0.5f);

            _cts = new();
            _token = _cts.Token;

            var builder = Disposable.CreateBuilder();
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
            await UniTask.Delay(_waitTimeAmount, false, PlayerLoopTiming.Update, _token);
            await UniTask.WaitUntil(() => IsBoardStop(), PlayerLoopTiming.Update, _token);
        }

        private bool IsBoardStop()
        {
            if (_checkGameBoardMovementTask.IsBoardLock)
                return false;

            if (_activateBoosterTask.ActiveBoosterCount > 0)
                return false;

            if (!_checkTargetTask.IsAllTargetsStopped())
                return false;

            return true;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
