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

        private IDisposable _messageDisposable;
        private IDisposable _disposable;

        public ComplimentTask(CharacterEmotion characterEmotion)
        {
            _complimentCounter = new();
            _delay = TimeSpan.FromSeconds(0.5f);
            _throttle = TimeSpan.FromSeconds(0.5f);
            _characterEmotion = characterEmotion;

            var messageBuilder = MessagePipe.DisposableBag.CreateBuilder();

            _complimentSubscriber = GlobalMessagePipe.GetSubscriber<ComplimentMessage>();
            _complimentSubscriber.Subscribe(_ => AddCounter())
                                 .AddTo(messageBuilder);

            _messageDisposable = messageBuilder.Build();

            var builder = Disposable.CreateBuilder();

            _complimentCounter.Pairwise().Where(pair => pair.Previous != pair.Current)
                              .Debounce(_throttle)
                              .Delay(_delay)
                              .Subscribe(pair => OnCounterEnd(pair.Current))
                              .AddTo(ref builder);

            _disposable = builder.Build();
        }

        private void AddCounter()
        {
            _complimentCounter.Value = _complimentCounter.Value + 1;
        }

        private void OnCounterEnd(int count)
        {
            if (count == 0)
                return;

            ComplimentEnum compliment = count switch
            {
                3 => ComplimentEnum.Good,
                5 => ComplimentEnum.Yummy,
                8 => ComplimentEnum.Super,
                _ => ComplimentEnum.None
            };

            if(compliment != ComplimentEnum.None)
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
