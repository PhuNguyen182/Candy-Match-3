using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies.Suggests;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using GlobalScripts.UpdateHandlerPattern;
using Random = UnityEngine.Random;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SuggestTask : IDisposable, IUpdateHandler
    {
        private readonly GridCellManager _gridCellManager;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly ISubscriber<DecreaseMoveMessage> _decreaseMoveSubscriber;

        private const float SuggestDelay = 4f;
        private const float SuggestInterval = 6f;
        private const float SuggestCooldown = 8f;

        private int _suggestCount = 0;
        private bool _suggestFlag = false;
        private float _suggestTimer = 0;

        private List<IItemSuggest> _itemSuggests;
        private InputProcessTask _inputProcessTask;
        private CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private IDisposable _messageDisposable;

        public bool IsActive { get; set; }

        public SuggestTask(GridCellManager gridCellManager, DetectMoveTask detectMoveTask)
        {
            _itemSuggests = new();
            _gridCellManager = gridCellManager;
            _detectMoveTask = detectMoveTask;

            DisposableBagBuilder messageBuilder = MessagePipe.DisposableBag.CreateBuilder();
            _decreaseMoveSubscriber = GlobalMessagePipe.GetSubscriber<DecreaseMoveMessage>();
            _decreaseMoveSubscriber.Subscribe(OnDecreaseMove).AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            IsActive = true;
            UpdateHandlerManager.Instance.AddUpdateBehaviour(this);
        }

        public void OnUpdate(float deltaTime)
        {
            if (!_checkGameBoardMovementTask.IsBoardLock)
            {
                _suggestTimer += Time.deltaTime;
                if(_suggestTimer > SuggestDelay && !_suggestFlag)
                {
                    Suggest(true);
                    _suggestFlag = true;
                }

                else if(_suggestTimer > SuggestInterval && _suggestTimer <= SuggestCooldown)
                {
                    Suggest(false);
                }

                else if(_suggestTimer > SuggestCooldown)
                {
                    _suggestTimer = 0;
                    _suggestFlag = false;
                }
            }

            else
            {
                _suggestTimer = 0;
                _suggestFlag = false;
            }
        }

        public void SetInputProcessTask(InputProcessTask inputProcessTask)
        {
            _inputProcessTask = inputProcessTask;
        }

        public void SetCheckGameBoardMovementTask(CheckGameBoardMovementTask checkGameBoardMovementTask)
        {
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
        }

        public void Suggest(bool isSuggest)
        {
            if (isSuggest)
                SearchSuggestions();

            Highlight(isSuggest);
        }

        private void SearchSuggestions()
        {
            _inputProcessTask.IsActive = false;

            if (_suggestCount == 0)
                _detectMoveTask.DetectPossibleMoves();

            if (_detectMoveTask.HasPossibleMove())
            {
                using (ListPool<AvailableSuggest>.Get(out List<AvailableSuggest> suggests))
                {
                    ClearSuggestedItems();
                    suggests.AddRange(_detectMoveTask.GetPossibleSwaps());

                    int count = suggests.Count;
                    int index = _suggestCount == 0 ? count - 1 
                                : Random.Range(count * 2 / 3, count);
                    AvailableSuggest detectedSuggest = suggests[index];
                    
                    for (int i = 0; i < detectedSuggest.Positions.Count; i++)
                    {
                        Vector3Int position = detectedSuggest.Positions[i];
                        IGridCell gridCell = _gridCellManager.Get(position);
                        IBlockItem blockItem = gridCell.BlockItem;

                        if (blockItem is IItemSuggest itemSuggest)
                            _itemSuggests.Add(itemSuggest);
                    }
                }

                _suggestCount = _suggestCount + 1;
                _inputProcessTask.IsActive = true;
            }
        }

        private void Highlight(bool isActive)
        {
            if (_itemSuggests.Count <= 0)
                return;
            
            _suggestTimer = 0;
            _suggestFlag = false;

            for (int i = 0; i < _itemSuggests.Count; i++)
                _itemSuggests[i].Highlight(isActive);

            if (!isActive)
                ClearSuggestedItems();
        }

        private void ClearSuggestedItems()
        {
            _itemSuggests.Clear();
        }

        private void OnDecreaseMove(DecreaseMoveMessage message)
        {
            if (message.CanDecrease)
            {
                ClearSuggest();
            }
        }

        public void ClearSuggest()
        {
            Highlight(false);
            ClearSuggestedItems();

            _suggestCount = 0;
        }

        public void Dispose()
        {
            ClearSuggest();
            _messageDisposable.Dispose();
        }
    }
}
