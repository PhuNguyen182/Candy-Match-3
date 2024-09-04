using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies.Suggests;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Random = UnityEngine.Random;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class SuggestTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly DetectMoveTask _detectMoveTask;

        private int _suggestCount = 0;
        private List<IItemSuggest> _itemSuggests;

        public SuggestTask(GridCellManager gridCellManager, DetectMoveTask detectMoveTask)
        {
            _gridCellManager = gridCellManager;
            _detectMoveTask = detectMoveTask;

            _itemSuggests = new();
        }

        public void Suggest(bool isSuggest)
        {
            if (isSuggest)
                SearchSuggestions();

            Highlight(isSuggest);
        }

        private void SearchSuggestions()
        {
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
                                : Random.Range(count / 2, count - 1);
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
            }
        }

        private void Highlight(bool isActive)
        {
            if (_itemSuggests.Count <= 0)
                return;

            for (int i = 0; i < _itemSuggests.Count; i++)
            {
                _itemSuggests[i].Highlight(isActive);
            }

            if (!isActive)
            {
                ClearSuggestedItems();
            }
        }

        private void ClearSuggestedItems()
        {
            _itemSuggests.Clear();
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
        }
    }
}
