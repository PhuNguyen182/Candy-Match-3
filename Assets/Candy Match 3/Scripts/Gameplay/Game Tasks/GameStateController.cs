using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            BuyMove,
            Quit
        }

        private readonly StateMachine<State, Trigger> _gameStateMachine;
        private readonly StateMachine<State, Trigger>.TriggerWithParameters<bool> _endGameTrigger;

        public GameStateController()
        {
            _gameStateMachine = new StateMachine<State, Trigger>(State.Start);
            _endGameTrigger = _gameStateMachine.SetTriggerParameters<bool>(Trigger.EndGame);

            _gameStateMachine.Configure(State.Start)
                             .Permit(Trigger.Play, State.Playing)
                             .OnEntry(StartGame);

            _gameStateMachine.Configure(State.Playing)
                             .OnEntryFrom(Trigger.Play, PlayGame)
                             .Permit(_endGameTrigger.Trigger, State.EndGame)
                             .Permit(Trigger.Quit, State.Quit);

            _gameStateMachine.Configure(State.EndGame)
                             .OnEntryFromAsync(_endGameTrigger, value => EndGame(value))
                             .Permit(Trigger.BuyMove, State.Playing)
                             .Permit(Trigger.Quit, State.Quit);

            _gameStateMachine.Configure(State.Quit)
                             .OnEntry(QuitGame);

            _gameStateMachine.Activate();
        }

        private void StartGame()
        {

        }

        private void PlayGame()
        {

        }

        private async UniTask EndGame(bool isWin)
        {
            await UniTask.CompletedTask;
        }

        private void QuitGame()
        {

        }

        public void Dispose()
        {
            _gameStateMachine.Deactivate();
        }
    }
}
