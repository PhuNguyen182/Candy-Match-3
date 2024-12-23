using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.SceneUtils;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using CandyMatch3.Scripts.GameData.LevelProgresses;
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
        private readonly ComplimentTask _complimentTask;
        private readonly InGameSettingPanel _settingSidePanel;
        private readonly SuggestTask _suggestTask;

        private readonly StateMachine<State, Trigger> _gameStateMachine;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<EndResult> _endGameTrigger;

        private const int DefaultContinueMove = 5;
        private const int DefaultContinuePrice = 300;

        private bool _levelIncreased = false;
        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private LevelModel _levelModel;

        public GameStateController(InputProcessTask inputProcessTask, CheckTargetTask checkTargetTask, StartGameTask startGameTask, ComplimentTask complimentTask
            , EndGameTask endGameTask, EndGameScreen endGameScreen, SuggestTask suggestTask, InGameSettingPanel settingSidePanel)
        {
            _endGameTask = endGameTask;
            _startGameTask = startGameTask;
            _checkTargetTask = checkTargetTask;
            _inputProcessTask = inputProcessTask;
            _endGameScreen = endGameScreen;
            _settingSidePanel = settingSidePanel;
            _suggestTask = suggestTask;
            _complimentTask = complimentTask;

            _cts = new();
            _token = _cts.Token;

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
                             .OnEntry(() => OnQuitGame().Forget());
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
            _complimentTask.IsEndGame = false;
            _settingSidePanel.SetButtonSettingParent(true);
        }

        private void PlayContinue()
        {
            SetPlayerActive(true);
            _complimentTask.IsEndGame = false;
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
            _complimentTask.IsEndGame = true;
            _endGameScreen.ShowBackground(true);
            _settingSidePanel.SetButtonSettingParent(false, _endGameScreen.transform);

            if (result == EndResult.Win)
            {
                if (!PlayGameConfig.Current.IsTestMode)
                {
                    int stars = _checkTargetTask.Stars;
                    int level = PlayGameConfig.Current.Level;
                    int currentLevel = GameDataManager.Instance.GetCurrentLevel();

                    // Save current win level immediately
                    if(level == currentLevel)
                    {
                        _levelIncreased = true;
                        GameDataManager.Instance.SetLevel(level + 1);
                    }
                    
                    GameDataManager.Instance.EarnResource(GameResourceType.Life, 1);
                    GameDataManager.Instance.AddLevelProgress(new LevelProgressNode
                    {
                        Level = level,
                        Stars = stars
                    });

                    // Preset back home config
                    BackHomeConfig.Current = new BackHomeConfig
                    {
                        Level = level,
                        Stars = stars,
                        EndResult = result,
                        LevelIncreased = _levelIncreased,
                    };
                }

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
            int stars = _checkTargetTask.Stars;
            int level = PlayGameConfig.Current.Level;

            BackHomeConfig.Current = new BackHomeConfig
            {
                EndResult = result,
                LevelIncreased = _levelIncreased,
                Level = level,
                Stars = stars,
            };

            _endGameScreen.ShowBackground(false);
            if (_gameStateMachine.CanFire(Trigger.Quit))
            {
                _gameStateMachine.Fire(Trigger.Quit);
            }
        }

        private async UniTask OnQuitGame()
        {
            PlayGameConfig.Reset();
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            await SceneBridge.LoadNextScene(SceneConstants.Mainhome);
        }

        public void Dispose()
        {
            _cts.Dispose();
            _gameStateMachine.Deactivate();
        }
    }
}
