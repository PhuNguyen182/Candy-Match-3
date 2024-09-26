using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies.Suggests;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using GlobalScripts.UpdateHandlerPattern;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SuggestTask : IDisposable, IUpdateHandler
    {
        private readonly GridCellManager _gridCellManager;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly MatchItemsTask _matchItemsTask;
        private readonly ISubscriber<DecreaseMoveMessage> _decreaseMoveSubscriber;

        private const float SuggestDelay = 4f;
        private const float SuggestInterval = 6f;
        private const float SuggestCooldown = 10f;

        private int _suggestCount = 0;
        private float _suggestTimer = 0;
        private bool _suggestFlag = false;

        private List<IItemSuggest> _itemSuggests;
        private InputProcessTask _inputProcessTask;
        private CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private IDisposable _messageDisposable;

        public bool IsActive { get; set; }

        public SuggestTask(GridCellManager gridCellManager, DetectMoveTask detectMoveTask, MatchItemsTask matchItemsTask)
        {
            _itemSuggests = new();
            _gridCellManager = gridCellManager;
            _detectMoveTask = detectMoveTask;
            _matchItemsTask = matchItemsTask;

            DisposableBagBuilder messageBuilder = MessagePipe.DisposableBag.CreateBuilder();
            _decreaseMoveSubscriber = GlobalMessagePipe.GetSubscriber<DecreaseMoveMessage>();
            _decreaseMoveSubscriber.Subscribe(OnDecreaseMove).AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            IsActive = true;
            UpdateHandlerManager.Instance.AddUpdateBehaviour(this);
        }

        public void OnUpdate(float deltaTime)
        {
            /*
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
                    ClearTimer();
                }
            }

            else
            {
                ClearTimer();
            }
            */
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

        public void ClearTimer()
        {
            _suggestTimer = 0;
            _suggestFlag = false;
        }

        private void SearchSuggestions()
        {
            _inputProcessTask.IsActive = false;

            if (_suggestCount == 0)
                _detectMoveTask.DetectPossibleMoves();

            if (_detectMoveTask.HasPossibleMove())
            {
                ClearSuggestedItems();
                AvailableSuggest suggest = _detectMoveTask.GetPossibleSwap(_suggestCount);
                AvailableSuggest detectedSuggest = ExportSuggestResult(suggest);

                if (detectedSuggest.Positions != null)
                {
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
            }

            else
            {
#if UNITY_EDITOR
                Debug.Log("No Possible Moves!");
#endif
            }

            _inputProcessTask.IsActive = true;
        }

        private void Highlight(bool isActive)
        {
            if (_itemSuggests.Count <= 0)
                return;

            ClearTimer();

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

        // Optimize modification
        private AvailableSuggest ExportSuggestResult(AvailableSuggest availableSuggest)
        {
            // Instead of taking all possible positions in one loop, just take the position
            // and direction with its score, then calculate available positions later
            Vector3Int originalPosition = availableSuggest.Position;
            Vector3Int switchedPosition = originalPosition + availableSuggest.Direction;

            if (availableSuggest.IsSwapWithBooster)
            {
                availableSuggest.Positions = new() 
                { 
                    originalPosition, 
                    switchedPosition 
                };

                return availableSuggest;
            }

            else
            {
                IGridCell gridCell1 = _gridCellManager.Get(originalPosition);
                IGridCell gridCell2 = _gridCellManager.Get(switchedPosition);
                
                MatchResult matchResult;
                _detectMoveTask.PseudoSwapItems(gridCell1, gridCell2);

                if (_matchItemsTask.IsMatchable(originalPosition, out matchResult))
                {
                    List<Vector3Int> positions = new(matchResult.MatchSequence);
                    int count = matchResult.MatchSequence.Count;
                    positions[count - 1] = gridCell2.GridPosition;
                    availableSuggest.Positions = positions;

                    _detectMoveTask.PseudoSwapItems(gridCell1, gridCell2);
                    return availableSuggest;
                }

                _detectMoveTask.PseudoSwapItems(gridCell1, gridCell2);
            }

            return new();
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
            UpdateHandlerManager.Instance.RemoveUpdateBehaviour(this);
        }
    }
}
