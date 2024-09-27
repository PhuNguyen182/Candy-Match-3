using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class EndGameTask : IDisposable
    {
        private readonly CheckTargetTask _checkTargetTask;
        private readonly CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private readonly ActivateBoosterTask _activateBoosterTask;
        private readonly EndGameScreen _endGameScreen;
        private readonly TimeSpan _waitTimeAmount;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private CheckGridTask _checkGridTask;
        private IDisposable _disposable;

        public EndGameTask(CheckTargetTask checkTargetTask, CheckGameBoardMovementTask checkGameBoardMovementTask
            , ActivateBoosterTask activateBoosterTask, EndGameScreen endGameScreen)
        {
            _checkTargetTask = checkTargetTask;
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
            _activateBoosterTask = activateBoosterTask;
            _endGameScreen = endGameScreen;
            _waitTimeAmount = TimeSpan.FromSeconds(0.3f);

            _cts = new();
            _token = _cts.Token;

            var builder = Disposable.CreateBuilder();
            _disposable = builder.Build();

            _checkTargetTask.SetEndGameTask(this);
        }

        public async UniTask OnWinGame()
        {
            int score = _checkTargetTask.Score;
            int stars = _checkTargetTask.Stars;
            _endGameScreen.WinSetScoreAndStars(score, stars);
            await UniTask.CompletedTask;
        }

        public async UniTask OnLoseGame()
        {
            int score = _checkTargetTask.Score;
            _endGameScreen.SetLoseScore(score);
            await UniTask.CompletedTask;
        }

        public async UniTask WaitAWhile()
        {
            await UniTask.Delay(_waitTimeAmount, false, PlayerLoopTiming.Update, _token);
        }

        public async UniTask WaitForBoardStop()
        {
            await UniTask.WaitUntil(() => IsBoardStop(), PlayerLoopTiming.Update, _token);
        }

        private bool IsBoardStop()
        {
            if (!_checkGridTask.CanCheck)
                return false;

            if (!_checkGameBoardMovementTask.AllGridsUnlocked)
                return false;

            if (_activateBoosterTask.ActiveBoosterCount > 0)
                return false;

            if (!_checkTargetTask.IsAllTargetsStopped())
                return false;

            return true;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
