using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using Cysharp.Threading.Tasks;
using Stateless;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class GameStateController : IDisposable
    {
        private enum State
        {
            Start,
            Playing,
            EndGame,
            Quit,
        }

        private enum Trigger
        {
            Play,
            EndGame,
            Continue,
            Quit
        }

        private readonly EndGameTask _endGameTask;
        private readonly InputProcessTask _inputProcessTask;
        private readonly CheckTargetTask _checkTargetTask;
        private readonly EndGameScreen _endGameScreen;
        private readonly SuggestTask _suggestTask;

        private readonly StateMachine<State, Trigger> _gameStateMachine;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<EndResult> _endGameTrigger;

        public GameStateController(InputProcessTask inputProcessTask, CheckTargetTask checkTargetTask
            , EndGameTask endGameTask, EndGameScreen endGameScreen, SuggestTask suggestTask)
        {
            _endGameTask = endGameTask;
            _checkTargetTask = checkTargetTask;
            _inputProcessTask = inputProcessTask;
            _endGameScreen = endGameScreen;
            _suggestTask = suggestTask;

            _checkTargetTask.OnEndGame = EndGame;

            _gameStateMachine = new StateMachine<State, Trigger>(State.Start);
            _endGameTrigger = _gameStateMachine.SetTriggerParameters<EndResult>(Trigger.EndGame);

            _gameStateMachine.Configure(State.Start)
                             .Permit(Trigger.Play, State.Playing)
                             .OnActivate(StartGame);

            _gameStateMachine.Configure(State.Playing)
                             .OnEntryFrom(Trigger.Play, PlayGame)
                             .OnEntryFrom(Trigger.Continue, PlayContinue)
                             .Permit(_endGameTrigger.Trigger, State.EndGame)
                             .Permit(Trigger.Quit, State.Quit);

            _gameStateMachine.Configure(State.EndGame)
                             .OnEntryFrom(_endGameTrigger, value => OnEndGame(value).Forget())
                             .Permit(Trigger.Continue, State.Playing)
                             .Permit(Trigger.Quit, State.Quit);

            _gameStateMachine.Configure(State.Quit)
                             .OnEntry(OnQuitGame);

            _gameStateMachine.Activate();
        }

        private void StartGame()
        {
            _suggestTask.IsActive = false;
            _inputProcessTask.IsActive = false;

            if (_gameStateMachine.CanFire(Trigger.Play))
            {
                _gameStateMachine.Fire(Trigger.Play);
            }
        }

        private void PlayGame()
        {
            _suggestTask.IsActive = true;
            _inputProcessTask.IsActive = true;
        }

        private void PlayContinue()
        {
            _suggestTask.IsActive = true;
            _inputProcessTask.IsActive = true;
        }

        private void EndGame(EndResult result)
        {
            if (_gameStateMachine.CanFire(_endGameTrigger.Trigger))
            {
                _gameStateMachine.Fire(_endGameTrigger, result);
            }
        }

        private async UniTask OnEndGame(EndResult result)
        {
            _suggestTask.Suggest(false);
            _suggestTask.IsActive = false;
            _inputProcessTask.IsActive = false;
            _endGameScreen.ShowBackground(true);

            if (result == EndResult.Win)
            {
                await _endGameTask.OnWinGame();
                await _endGameScreen.ShowWinGame();
                QuitGame(result);
            }

            else
            {
                await _endGameTask.OnLoseGame();
                List<TargetElement> remainTarget = _checkTargetTask.GetRemainTargets();

                await _endGameScreen.ShowLoseGame(remainTarget);
                bool canContinue = await _endGameScreen.ShowContinue();

                if (canContinue)
                {
                    AddMove();
                }

                else
                {
                    QuitGame(result);
                }
            }
        }

        private void AddMove()
        {
            _checkTargetTask.AddMove(5);
            _endGameScreen.ShowBackground(false);

            if (_gameStateMachine.CanFire(Trigger.Continue))
            {
                _gameStateMachine.Fire(Trigger.Continue);
            }
        }

        private void QuitGame(EndResult result)
        {
#if UNITY_EDITOR
            Debug.Log($"End result: {result}");
#endif
            _endGameScreen.ShowBackground(false);

            if (_gameStateMachine.CanFire(Trigger.Quit))
            {
                _gameStateMachine.Fire(Trigger.Quit);
            }
        }

        private void OnQuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("Quit Level!");
#endif
        }

        public void Dispose()
        {
            _gameStateMachine.Deactivate();
        }
    }
}
