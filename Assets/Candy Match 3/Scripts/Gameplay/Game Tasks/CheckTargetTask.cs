using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.Enums;
using GlobalScripts.Utils;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CheckTargetTask : IDisposable
    {
        private readonly TargetDatabase _targetDatabase;
        private readonly MainGamePanel _mainGamePanel;

        private readonly ISubscriber<DecreaseMoveMessage> _decreaseMoveSubscriber;
        private readonly ISubscriber<AsyncMessage<MoveTargetData>> _moveToTargetSubscriber;
        private readonly ISubscriber<DecreaseTargetMessage> _decreaseTargetSubscriber;

        private int _score = 0;
        private int _moveCount = 0;

        private HashSet<UniTask> _moveToTargetTasks;
        private List<LevelTargetData> _levelTargetDatas;
        private Dictionary<TargetEnum, TargetElement> _targetCollections;
        private Dictionary<TargetEnum, int> _targetCounts;
        
        private ScoreRule _scoreRule;
        private EndGameTask _endGameTask;
        private IDisposable _disposable;

        public Action<bool> OnEndGame;

        public CheckTargetTask(TargetDatabase targetDatabase, MainGamePanel mainGameScreen)
        {
            _targetDatabase = targetDatabase;
            _mainGamePanel = mainGameScreen;
            _targetDatabase.Initialize();

            _moveToTargetTasks = new();

            var builder = MessagePipe.DisposableBag.CreateBuilder();            
            _decreaseMoveSubscriber = GlobalMessagePipe.GetSubscriber<DecreaseMoveMessage>();
            _moveToTargetSubscriber = GlobalMessagePipe.GetSubscriber<AsyncMessage<MoveTargetData>>();
            _decreaseTargetSubscriber = GlobalMessagePipe.GetSubscriber<DecreaseTargetMessage>();

            _decreaseMoveSubscriber.Subscribe(DecreaseMove).AddTo(builder);
            _moveToTargetSubscriber.Subscribe(InspectTargetInfo).AddTo(builder);
            _decreaseTargetSubscriber.Subscribe(DecreaseTarget).AddTo(builder);

            _disposable = builder.Build();
        }

        public void InitLevelTarget(LevelModel levelModel)
        {
            _moveCount = levelModel.TargetMove;
            _scoreRule = levelModel.ScoreRule;
            _levelTargetDatas = levelModel.LevelTargetData;

            _targetCounts = new();
            List<TargetView> targetViews = new();
            List<TargetStats> targetStats = new();

            for (int i = 0; i < _levelTargetDatas.Count; i++)
            {
                int amount = _levelTargetDatas[i].DataValue.TargetAmount;
                TargetEnum targetType = _levelTargetDatas[i].DataValue.Target;
                Sprite icon = _targetDatabase.GetTargetIcon(targetType);

                TargetView targetView = new TargetView
                {
                    TargetType = targetType,
                    Icon = icon
                };

                TargetStats targetStat = new TargetStats
                {
                    Amount = amount,
                };

                targetViews.Add(targetView);
                targetStats.Add(targetStat);
                _targetCounts.Add(targetType, amount);
            }

            UpdateMove();
            _mainGamePanel.InitTargets(targetViews, targetStats);
            _mainGamePanel.InitScore(levelModel.ScoreRule);
            _targetCollections = _mainGamePanel.TargetElements;
        }

        public bool IsAllTargetsStopped()
        {
            return _moveToTargetTasks.Count <= 0;
        }

        public void CheckEndGame()
        {
            int remainCount = 0;
            foreach (var targetCount in _targetCounts)
            {
                remainCount = remainCount + targetCount.Value;
            }

            if (_moveCount >= 0 && remainCount <= 0)
            {
                OnEndGame?.Invoke(true);
            }

            else if(_moveCount == 0 && remainCount > 0)
            {
                OnEndGame?.Invoke(false);
            }
        }

        public void AddScore(int score)
        {
            _score = _score + score;
            _mainGamePanel.UpdateScore(_score);
        }

        public void UpdateAll()
        {
            foreach (var target in _targetCollections)
            {
                UpdateTarget(target.Key, false);
            }
        }

        public void AddMove(int move)
        {
            _moveCount = _moveCount + move;
            UpdateMove();
            CheckEndGame();
        }

        public void SetEndGameTask(EndGameTask endGameTask)
        {
            _endGameTask = endGameTask;
        }

        private void UpdateMove()
        {
            _mainGamePanel.UpdateMove(_moveCount);
        }

        private void UpdateTarget(TargetEnum targetType, bool isDecrease)
        {
            if (_targetCounts.TryGetValue(targetType, out var target))
            {
                if (isDecrease)
                {
                    target = target - 1;
                    _targetCounts[targetType] = target;
                }

                var targetCell = _targetCollections[targetType];
                targetCell.UpdateTargetCount(new TargetStats
                {
                    Amount = target,
                    IsCompleted = target <= 0,
                    IsFailed = target > 0 && _moveCount <= 0
                });

                if (isDecrease)
                    targetCell.PlayTargetAnimation();
            }
        }

        private void DecreaseTarget(DecreaseTargetMessage message)
        {
            if (message.HasMoveTask)
            {
                UniTask task = message.Task;
                _moveToTargetTasks.Add(task);

                var moveTask = task.ContinueWith(() =>
                {
                    UpdateTarget(message.TargetType, true);
                    _moveToTargetTasks.Remove(task);
                });
            }

            else
                UpdateTarget(message.TargetType, true);
        }

        private void InspectTargetInfo(AsyncMessage<MoveTargetData> message)
        {
            TargetEnum targetType = message.Data.TargetType;
            if(_targetCollections.TryGetValue(targetType, out var target))
            {
                Vector3 position = target.transform.position;
                int remainTargetCount = _targetCounts[targetType];
                position.z = 0;

                MoveTargetData moveTargetData = new MoveTargetData
                {
                    TargetType = targetType,
                    IsCompleted = remainTargetCount <= 0,
                    Destination = position
                };

                MessageBrokerUtils<MoveTargetData>.SendBackMessage(message, moveTargetData);
            }
        }

        private void DecreaseMove(DecreaseMoveMessage message)
        {
            if (message.CanDecrease)
            {
                _moveCount = _moveCount - 1;
                UpdateMove();
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();

            _targetCounts.Clear();
            _moveToTargetTasks.Clear();
            _levelTargetDatas.Clear();
            _targetCollections.Clear();
        }
    }
}
