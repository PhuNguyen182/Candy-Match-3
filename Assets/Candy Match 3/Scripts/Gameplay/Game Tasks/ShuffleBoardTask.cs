using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class ShuffleBoardTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly InputProcessTask _inputProcessTask;
        private readonly DetectMoveTask _detectMoveTask;
        private readonly SuggestTask _suggestTask;
        private readonly FillBoardTask _fillBoardTask;

        private List<Vector3Int> _activePositions;
        private List<Vector3Int> _shuffleableCells;

        private const int MaxRetryTime = 1000;
        private const float ItemTransformDelay = 0.01f;

        public ShuffleBoardTask(GridCellManager gridCellManager, InputProcessTask inputProcessTask
            , DetectMoveTask detectMoveTask, SuggestTask suggestTask, FillBoardTask fillBoardTask)
        {
            _gridCellManager = gridCellManager;
            _inputProcessTask = inputProcessTask;
            _detectMoveTask = detectMoveTask;
            _suggestTask = suggestTask;
            _fillBoardTask = fillBoardTask;

            _shuffleableCells = new();
            PreloadPopup();
        }

        public void BuildActivePositions()
        {
            _activePositions = _gridCellManager.GetActivePositions().ToList();
        }

        public async UniTask CheckShuffleBoard()
        {
            _detectMoveTask.DetectPossibleMoves();
            if (_detectMoveTask.HasPossibleMove())
                return;

            _inputProcessTask.IsActive = false;
            var popup = await ShufflePopup.CreateFromAddress(CommonPopupPaths.ShufflePopupPath);
            await popup.ClosePopup();
            await Shuffle(false);

            _inputProcessTask.IsActive = true;
        }

        public void Shuffle()
        {
            _detectMoveTask.DetectPossibleMoves();
            if (_detectMoveTask.HasPossibleMove())
                return;

            bool isShuffleable = TryShuffle();
            if (isShuffleable)
                TransformItems(true).Forget();
        }

        public async UniTask Shuffle(bool immediately)
        {
            SetSuggestActive(false);
            bool canShuffle = TryShuffle();

            if (canShuffle)
                await TransformItems(immediately);

            SetSuggestActive(true);
        }

        private bool TryShuffle()
        {
            int shuffleCount = 0;
            CollectShuffleableCell();

            while (shuffleCount < MaxRetryTime)
            {
                _fillBoardTask.BuildShuffle(_shuffleableCells);
                _detectMoveTask.DetectPossibleMoves();

                if (_detectMoveTask.HasPossibleMove())
                    return true;

                shuffleCount = shuffleCount + 1;
            }

            return false;
        }

        private void CollectShuffleableCell()
        {
            _shuffleableCells.Clear();
            IGridCell gridCell = null;

            for (int i = 0; i < _activePositions.Count; i++)
            {
                gridCell = _gridCellManager.Get(_activePositions[i]);

                if (!gridCell.HasItem)
                    continue;

                if (gridCell.CandyColor == CandyColor.None)
                    continue;

                if (!gridCell.BlockItem.IsMatchable)
                    continue;

                if (!gridCell.IsMoveable)
                    continue;

                if (gridCell.BlockItem is IBooster)
                    continue;

                _shuffleableCells.Add(_activePositions[i]);
            }
        }

        private async UniTask TransformItems(bool immediately)
        {
            if (immediately)
            {
                for (int i = 0; i < _shuffleableCells.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(_shuffleableCells[i]);
                    IItemTransform itemTransform = gridCell.BlockItem as IItemTransform;
                    itemTransform.TransformImmediately();
                }
            }

            else
            {
                using (ListPool<UniTask>.Get(out List<UniTask> transformTasks))
                {
                    EffectManager.Instance.PlayShuffleEffect();
                    EffectManager.Instance.PlaySoundEffect(SoundEffectType.Shuffle);

                    for (int i = 0; i < _shuffleableCells.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(_shuffleableCells[i]);
                        IItemTransform itemTransform = gridCell.BlockItem as IItemTransform;
                        transformTasks.Add(itemTransform.Transform(i * ItemTransformDelay));
                    }

                    await UniTask.WhenAll(transformTasks);
                }
            }
        }

        private void PreloadPopup()
        {
            ShufflePopup.PreloadFromAddress(CommonPopupPaths.ShufflePopupPath).Forget();
        }

        private void SetSuggestActive(bool active)
        {
            if (!active)
            {
                _suggestTask.Suggest(false);
                _suggestTask.ClearSuggest();
            }

            _suggestTask.IsActive = active;
        }

        public void Dispose()
        {
            _activePositions.Clear();
            _shuffleableCells.Clear();
        }
    }
}
