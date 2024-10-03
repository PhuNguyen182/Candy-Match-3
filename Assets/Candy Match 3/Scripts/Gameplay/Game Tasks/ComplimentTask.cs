using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Common.Messages;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class ComplimentTask : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly TimeSpan _throttle;
        private readonly CharacterEmotion _characterEmotion;
        private readonly ReactiveProperty<int> _complimentCounter;
        private readonly ISubscriber<ComplimentMessage> _complimentSubscriber;

        private CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private IDisposable _messageDisposable;
        private IDisposable _disposable;

        public bool IsEndGame { get; set; }

        public ComplimentTask(CharacterEmotion characterEmotion)
        {
            _complimentCounter = new();
            _delay = TimeSpan.FromSeconds(1f);
            _throttle = TimeSpan.FromSeconds(0.334f);
            _characterEmotion = characterEmotion;

            var messageBuilder = MessagePipe.DisposableBag.CreateBuilder();

            _complimentSubscriber = GlobalMessagePipe.GetSubscriber<ComplimentMessage>();
            _complimentSubscriber.Subscribe(_ => AddCounter()).AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            var builder = Disposable.CreateBuilder();
            _complimentCounter.Pairwise().Where(ComplimentPredicate)
                              .Debounce(_throttle).Delay(_delay)
                              .Subscribe(pair => OnCounterEnd(pair.Current))
                              .AddTo(ref builder);

            _disposable = builder.Build();
        }

        public void SetCheckGameBoardMovementTask(CheckGameBoardMovementTask checkGameBoardMovementTask)
        {
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
        }

        private void AddCounter()
        {
            _complimentCounter.Value = _complimentCounter.Value + 1;
        }

        private bool ComplimentPredicate((int Previous, int Current) pair)
        {
            return pair.Previous != pair.Current;
        }

        private void OnCounterEnd(int count)
        {
            if (IsEndGame)
                return;

            if (!_checkGameBoardMovementTask.AllGridsUnlocked)
                return;

            if (count == 0)
            {
                _complimentCounter.Value = 0;
                return;
            }

            ComplimentEnum compliment = count switch
            {
                3 => ComplimentEnum.Good,
                5 => ComplimentEnum.Yummy,
                8 => ComplimentEnum.Super,
                _ => ComplimentEnum.None
            };

            if (count > 8)
                compliment = ComplimentEnum.Super;

            if (compliment != ComplimentEnum.None)
            {
                _characterEmotion.ShowHappyState();
                EffectManager.Instance.ShowCompliment(compliment);
            }

            _complimentCounter.Value = 0;
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _messageDisposable.Dispose();
        }
    }
}
