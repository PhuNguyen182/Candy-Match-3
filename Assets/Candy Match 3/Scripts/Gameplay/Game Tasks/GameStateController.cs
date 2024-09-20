using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using CandyMatch3.Scripts.Gameplay.Models;
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

        private readonly StartGameTask _startGameTask;
        private readonly EndGameTask _endGameTask;
        private readonly InputProcessTask _inputProcessTask;
        private readonly CheckTargetTask _checkTargetTask;
        private readonly EndGameScreen _endGameScreen;
        private readonly InGameSettingPanel _settingSidePanel;
        private readonly SuggestTask _suggestTask;

        private readonly StateMachine<State, Trigger> _gameStateMachine;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<EndResult> _endGameTrigger;
        private LevelModel _levelModel;

        private const int DefaultContinueMove = 5;
        private const int DefaultContinuePrice = 300;

        public GameStateController(InputProcessTask inputProcessTask, CheckTargetTask checkTargetTask, StartGameTask startGameTask
            , EndGameTask endGameTask, EndGameScreen endGameScreen, SuggestTask suggestTask, InGameSettingPanel settingSidePanel)
        {
            _endGameTask = endGameTask;
            _startGameTask = startGameTask;
            _checkTargetTask = checkTargetTask;
            _inputProcessTask = inputProcessTask;
            _endGameScreen = endGameScreen;
            _settingSidePanel = settingSidePanel;
            _suggestTask = suggestTask;

            _checkTargetTask.OnEndGame = EndGame;
            _settingSidePanel.OnSetting = SetPlayerActive;
            _settingSidePanel.QuitPopup.OnPlayerQuit = OnPlayerQuitPopup;

            _gameStateMachine = new StateMachine<State, Trigger>(State.Start);
            _endGameTrigger = _gameStateMachine.SetTriggerParameters<EndResult>(Trigger.EndGame);

            _gameStateMachine.Configure(State.Start)
                             .Permit(Trigger.Play, State.Playing)
                             .OnActivate(() => Ready().Forget());

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
        }

        public void SetLevelModel(LevelModel levelModel)
        {
            _levelModel = levelModel;
        }

        public void StartGame()
        {
            SetUpEndgame();
            _gameStateMachine.Activate();
        }

        private void SetUpEndgame()
        {
            _endGameScreen.SetContinuePriceAndMove(DefaultContinuePrice, DefaultContinueMove);
        }

        private async UniTask Ready()
        {
            SetPlayerActive(false);
            await _startGameTask.ShowLevelStart(_levelModel);

            if (_gameStateMachine.CanFire(Trigger.Play))
            {
                _gameStateMachine.Fire(Trigger.Play);
            }
        }

        private void PlayGame()
        {
            SetPlayerActive(true);
            _settingSidePanel.SetButtonSettingParent(true);
        }

        private void PlayContinue()
        {
            SetPlayerActive(true);
            _settingSidePanel.SetButtonSettingParent(true);
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
            SetPlayerActive(false);
            _endGameScreen.ShowBackground(true);
            _settingSidePanel.SetButtonSettingParent(false, _endGameScreen.transform);

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

        private void SetPlayerActive(bool isActive)
        {
            if (!isActive)
                _suggestTask.Suggest(false);

            _suggestTask.IsActive = isActive;
            _inputProcessTask.IsActive = isActive;
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

        private void OnPlayerQuitPopup()
        {
            QuitGame(EndResult.Lose);
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
