using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster;
using CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Mainhome.UI.Shops;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class InGameBoosterTasks : IDisposable
    {
        private readonly InputProcessTask _inputProcessTask;
        private readonly BreakBoosterTask _breakBoosterTask;
        private readonly BlastBoosterTask _blastBoosterTask;
        private readonly PlaceBoosterTask _placeBoosterTask;
        private readonly InGameBoosterPanel _inGameBoosterPanel;
        private readonly InGameBoosterPackDatabase _inGameBoosterPackDatabase;
        private readonly SwapItemTask _swapItemTask;
        private readonly SuggestTask _suggestTask;

        private CheckGridTask _checkGridTask;
        private CheckGameBoardMovementTask _checkGameBoardMovementTask;
        private readonly ISubscriber<AddInGameBoosterMessage> _addBoosterSubscriber;
        private readonly ISubscriber<UseInGameBoosterMessage> _useBoosterSubscriber;
        private Dictionary<InGameBoosterType, ReactiveProperty<int>> _boosters;

        private IDisposable _messageDisposable;
        private IDisposable _boosterDisposable;
        private IDisposable _disposable;

        public bool IsBoosterUsed { get; set; }
        public InGameBoosterType CurrentBooster { get; private set; }

        public InGameBoosterTasks(InputProcessTask inputProcessTask, GridCellManager gridCellManager, BreakGridTask breakGridTask
            , SuggestTask suggestTask, ExplodeItemTask explodeItemTask , SwapItemTask swapItemTask
            , ActivateBoosterTask activateBoosterTask, ComboBoosterHandleTask comboBoosterHandleTask, ItemManager itemManager
            , InGameBoosterPanel inGameBoosterPanel, InGameBoosterPackDatabase inGameBoosterPackDatabase)
        {
            _suggestTask = suggestTask;
            _swapItemTask = swapItemTask;
            _inputProcessTask = inputProcessTask;
            _breakBoosterTask = new(breakGridTask, explodeItemTask);
            _blastBoosterTask = new(gridCellManager, breakGridTask, explodeItemTask);
            _placeBoosterTask = new(gridCellManager, breakGridTask, activateBoosterTask, itemManager
                                    , comboBoosterHandleTask.ColorfulStripedBoosterTask
                                    , comboBoosterHandleTask.ColorfulWrappedBoosterTask);

            _inGameBoosterPanel = inGameBoosterPanel;
            _inGameBoosterPackDatabase = inGameBoosterPackDatabase;

            var messageBuilder = MessagePipe.DisposableBag.CreateBuilder();
            _addBoosterSubscriber = GlobalMessagePipe.GetSubscriber<AddInGameBoosterMessage>();
            _addBoosterSubscriber.Subscribe(AddBooster).AddTo(messageBuilder);

            _useBoosterSubscriber = GlobalMessagePipe.GetSubscriber<UseInGameBoosterMessage>();
            _useBoosterSubscriber.Subscribe(AfterUseBooster).AddTo(messageBuilder);
            _messageDisposable = messageBuilder.Build();

            var builder = Disposable.CreateBuilder();
            _blastBoosterTask.AddTo(ref builder);
            _placeBoosterTask.AddTo(ref builder);
            _disposable = builder.Build();

            PreloadPopups();
        }

        public void SetCheckBoardMovementTask(CheckGameBoardMovementTask checkGameBoardMovementTask)
        {
            _checkGameBoardMovementTask = checkGameBoardMovementTask;
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
            _blastBoosterTask.SetCheckGridTask(checkGridTask);
        }

        public void InitBoosters(List<InGameBoosterModel> boosterModels)
        {
            _boosters ??= new();

            if (!boosterModels.Exists(booster => booster.BoosterType == InGameBoosterType.Break))
                boosterModels.Add(new InGameBoosterModel { Amount = 0, BoosterType = InGameBoosterType.Break });

            if (!boosterModels.Exists(booster => booster.BoosterType == InGameBoosterType.Blast))
                boosterModels.Add(new InGameBoosterModel { Amount = 0, BoosterType = InGameBoosterType.Blast });

            if (!boosterModels.Exists(booster => booster.BoosterType == InGameBoosterType.Swap))
                boosterModels.Add(new InGameBoosterModel { Amount = 0, BoosterType = InGameBoosterType.Swap });

            if (!boosterModels.Exists(booster => booster.BoosterType == InGameBoosterType.Colorful))
                boosterModels.Add(new InGameBoosterModel { Amount = 0, BoosterType = InGameBoosterType.Colorful });

            using (ListPool<IDisposable>.Get(out List<IDisposable> boosterDisposables))
            {
                _boosters = boosterModels.ToDictionary(booster => booster.BoosterType, booster =>
                {
                    ReactiveProperty<int> boosterAmount = new(0);
                    BoosterButton boosterButton = _inGameBoosterPanel.GetBoosterButtonByType(booster.BoosterType);

                    boosterAmount.Subscribe(value => boosterButton.SetBoosterCount(value));
                    IDisposable d1 = boosterButton.OnClickObserver.Where(value => (boosterAmount.Value > 0 || value.IsFree) 
                                                  && !value.IsActive && _inputProcessTask.IsActive && !_checkGameBoardMovementTask.IsBoardLock)
                                                  .Subscribe(value => EnableBooster(booster.BoosterType));

                    IDisposable d2 = boosterButton.OnClickObserver.Where(value => boosterAmount.Value <= 0 && !value.IsFree && !value.IsActive 
                                                  && _inputProcessTask.IsActive && !_checkGameBoardMovementTask.IsBoardLock)
                                                  .Subscribe(value => ShowBuyBoosterPopup(booster.BoosterType).Forget());

                    boosterDisposables.Add(d1);
                    boosterDisposables.Add(d2);
                    boosterAmount.Value = booster.Amount;
                    return boosterAmount;
                });

                _boosterDisposable = Disposable.Combine(boosterDisposables.ToArray());
            }
        }

        public UniTask ActivatePointBooster(Vector3Int position, InGameBoosterType inGameBoosterType)
        {
            _checkGridTask.CanCheck = false;
            _inputProcessTask.IsActive = false;
            UniTask boosterTask = UniTask.CompletedTask;

            switch (inGameBoosterType)
            {
                case InGameBoosterType.Break:
                    boosterTask = ActivateBreakBooster(position);
                    break;
                case InGameBoosterType.Blast:
                    boosterTask = ActivateBlastBooster(position);
                    break;
                case InGameBoosterType.Colorful:
                    boosterTask = ActivatePlaceBooster(position);
                    break;
            }

            return boosterTask.ContinueWith(() =>
            {
                _checkGridTask.CanCheck = true;
                _inputProcessTask.IsActive = true;
            });
        }

        public UniTask ActivateSwapBooster(Vector3Int fromPosition, Vector3Int toPosition)
        {
            return _swapItemTask.SwapForward(fromPosition, toPosition);
        }

        private UniTask ActivateBreakBooster(Vector3Int position)
        {
            return _breakBoosterTask.Activate(position);
        }

        private UniTask ActivateBlastBooster(Vector3Int position)
        {
            return _blastBoosterTask.Activate(position);
        }

        private UniTask ActivatePlaceBooster(Vector3Int position)
        {
            return _placeBoosterTask.Activate(position);
        }

        private void AfterUseBooster(UseInGameBoosterMessage message)
        {
            if (!IsBoosterUsed)
                return;

            IsBoosterUsed = false;
            _boosters[message.BoosterType].Value--;
            CurrentBooster = InGameBoosterType.None;

            SetSuggestActive(true);
            _inGameBoosterPanel.SetBoosterPanelActive(false).Forget();
        }

        private void AddBooster(AddInGameBoosterMessage message)
        {
            _boosters[message.BoosterType].Value += message.Amount;
        }

        private void EnableBooster(InGameBoosterType boosterType)
        {
            IsBoosterUsed = true;
            CurrentBooster = boosterType;

            SetSuggestActive(false);
            _inGameBoosterPanel.ShowBoosterMessage(boosterType);
            _inGameBoosterPanel.SetBoosterPanelActive(true).Forget();
        }

        private async UniTask ShowBuyBoosterPopup(InGameBoosterType boosterType)
        {
            SetSuggestActive(false);
            _inputProcessTask.IsActive = false;

            var popup = await InGameBoosterPopup.CreateFromAddress(CommonPopupPaths.BuyBoosterPopupPath);
            InGameBoosterPack boosterPack = _inGameBoosterPackDatabase.BoosterPackCollections[boosterType];

            popup.SetBoosterInfo(boosterType);
            popup.SetBoosterPack(boosterPack);
            popup.UnblockInput = OnBuyBoosterPopupUnblockInput;
            popup.BlockInput = OnBuyBoosterPopupBlockInput;
        }

        private void OnBuyBoosterPopupBlockInput()
        {
            _suggestTask.Suggest(false);
            SetSuggestActive(false);
            _inputProcessTask.IsActive = false;
        }

        private void OnBuyBoosterPopupUnblockInput()
        {
            SetSuggestActive(true);
            _inputProcessTask.IsActive = true;
        }

        private void PreloadPopups()
        {
            ShopPopup.PreloadFromAddress(CommonPopupPaths.ShopPopupPath).Forget();
            InGameBoosterPopup.PreloadFromAddress(CommonPopupPaths.BuyBoosterPopupPath).Forget();
        }

        private void SetSuggestActive(bool isActive)
        {
            if (!isActive)
            {
                _suggestTask.ClearTimer();
                _suggestTask.Suggest(false);
            }

            _suggestTask.IsActive = isActive;
        }

        public void Dispose()
        {
            _disposable.Dispose();
            _boosterDisposable?.Dispose();
            _messageDisposable.Dispose();
        }
    }
}
