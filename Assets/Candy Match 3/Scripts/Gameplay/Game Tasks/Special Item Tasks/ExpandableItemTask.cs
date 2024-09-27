using R3;
using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.SpecialItemTasks
{
    public class ExpandableItemTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly GridCellManager _gridCellManager;

        private readonly ISubscriber<ExpandMessage> _expandSubscriber;
        private readonly ISubscriber<BreakExpandableMessage> _breakExpandSubscriber;
        private readonly ISubscriber<BoardStopMessage> _boardStopSubscriber;

        private bool _canExpand = false;
        private List<Vector3Int> _adjacentSteps;
        private HashSet<Vector3Int> _expandableItemPositions;
        private CheckGridTask _checkGridTask;
        private IDisposable _disposable;

        private CancellationToken _token;
        private CancellationTokenSource _cts;

        public ExpandableItemTask(GridCellManager gridCellManager, ItemManager itemManager, BreakGridTask breakGridTask)
        {
            _itemManager = itemManager;
            _expandableItemPositions = new();
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;

            var builder = MessagePipe.DisposableBag.CreateBuilder();

            _expandSubscriber = GlobalMessagePipe.GetSubscriber<ExpandMessage>();
            _expandSubscriber.Subscribe(ReceiveExpandMessage).AddTo(builder);

            _breakExpandSubscriber = GlobalMessagePipe.GetSubscriber<BreakExpandableMessage>();
            _breakExpandSubscriber.Subscribe(BreakPosition).AddTo(builder);

            _boardStopSubscriber = GlobalMessagePipe.GetSubscriber<BoardStopMessage>();
            _boardStopSubscriber.Subscribe(message => OnBoardStop(message).Forget()).AddTo(builder);

            _disposable = builder.Build();

            _cts = new();
            _token = _cts.Token;

            _adjacentSteps = new()
            {
                Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
            };
        }

        public void AddExpandablePosition(Vector3Int position)
        {
            _expandableItemPositions.Add(position);
        }

        public void SetCheckGridTask(CheckGridTask checkGridTask)
        {
            _checkGridTask = checkGridTask;
        }

        private void BreakPosition(BreakExpandableMessage message)
        {
            _expandableItemPositions.Remove(message.Position);
        }

        private void ReceiveExpandMessage(ExpandMessage message)
        {
            if (!message.CanExpand)
                _canExpand = false;
        }

        private async UniTask OnBoardStop(BoardStopMessage message)
        {
            // If all items on board are stopped, check and expand chocolate
            await UniTask.DelayFrame(3, PlayerLoopTiming.FixedUpdate, _token);

            if (message.IsStopped && _checkGridTask.CanCheck)
            {
                Expand();
            }
        }

        private void Expand()
        {
            int count = 0;
            int maxCount = _expandableItemPositions.Count;
            
            if (maxCount <= 0)
                return;

            if (!_canExpand)
            {
                _canExpand = true;
                return;
            }

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> expandablePositions))
            {
                using var flagPool = HashSetPool<int>.Get(out HashSet<int> flags);
                while (expandablePositions.Count <= 0 && count < maxCount)
                {
                    // Use flags to ensure no random index was double check
                    int randIndex = Random.Range(0, maxCount);
                    if(flags.Contains(randIndex))
                    {
                        count = count + 1;
                        continue;
                    }

                    flags.Add(randIndex);
                    Vector3Int randPosition = _expandableItemPositions.ElementAt(randIndex);
                    expandablePositions = GetExpandable(randPosition);
                    count = count + 1;
                }

                int expandableCount = expandablePositions.Count;
                if (expandableCount > 0)
                {
                    int randIndex = Random.Range(0, expandableCount);
                    Vector3Int position = expandablePositions[randIndex];
                    IGridCell replaceCell = _gridCellManager.Get(position);

                    _breakGridTask.ReleaseGridCell(replaceCell);
                    replaceCell.LockStates = LockStates.Replacing;

                    _itemManager.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            HealthPoint = 1,
                            ItemType = ItemType.Chocolate,
                            ItemColor = CandyColor.None,
                        }
                    });

                    _canExpand = false;
                    AddExpandablePosition(position);

                    if (replaceCell.BlockItem is IExpandableItem expandable)
                        expandable.Expand(position);

                    replaceCell.LockStates = LockStates.None;
                }
            }
        }

        private List<Vector3Int> GetExpandable(Vector3Int position)
        {
            List<Vector3Int> expandablePositions = new();
            for (int i = 0; i < _adjacentSteps.Count; i++)
            {
                Vector3Int checkPosition = position + _adjacentSteps[i];
                IGridCell checkGridCell = _gridCellManager.Get(checkPosition);

                if (checkGridCell == null || !checkGridCell.HasItem)
                    continue;

                if (checkGridCell.IsLocked || !checkGridCell.BlockItem.Replacable)
                    continue;

                expandablePositions.Add(checkPosition);
            }

            return expandablePositions;
        }

        public void Dispose()
        {
            _cts.Dispose();
            _expandableItemPositions.Clear();
            _disposable.Dispose();
            _adjacentSteps.Clear();
        }
    }
}
