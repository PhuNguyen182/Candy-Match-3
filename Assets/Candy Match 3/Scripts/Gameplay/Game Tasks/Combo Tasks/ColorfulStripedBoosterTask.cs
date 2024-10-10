using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.ComboTasks
{
    public class ColorfulStripedBoosterTask : IDisposable
    {
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;
        private readonly BreakGridTask _breakGridTask;
        private readonly ColorfulBoosterTask _colorfulBoosterTask;
        private readonly ActivateBoosterTask _activateBoosterTask;
        private readonly ColorfulFireray _colorfulFireray;

        private CancellationToken _token;
        private CancellationTokenSource _cts;
        private Vector3Int _errorPosition;

        public ColorfulStripedBoosterTask(ItemManager itemManager, GridCellManager gridCellManager, BreakGridTask breakGridTask
            , ActivateBoosterTask activateBoosterTask, ColorfulFireray colorfulFireray)
        {
            _itemManager = itemManager;
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _activateBoosterTask = activateBoosterTask;
            _colorfulBoosterTask = _activateBoosterTask.ColorfulBoosterTask;
            _colorfulFireray = colorfulFireray;

            _cts = new();
            _token = _cts.Token;
            _errorPosition = new(int.MinValue, int.MaxValue);
        }

        public async UniTask PlaceAndActivate(IGridCell gridCell, IBlockItem colorBooster)
        {
            CandyColor candyColor = colorBooster.CandyColor;
            IBooster activeBooster = gridCell.BlockItem as IBooster;
            gridCell.LockStates = LockStates.Preparing;
            
            activeBooster.IsActivated = true;
            await activeBooster.Activate();

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                PlayEffect();
                positions.AddRange(_colorfulBoosterTask.FindPositionWithColor(candyColor));
                using (ListPool<UniTask>.Get(out List<UniTask> fireTasks))
                {
                    Vector3 startPosition = gridCell.WorldPosition;
                    for (int i = 0; i < positions.Count; i++)
                    {
                        fireTasks.Add(FireItemCatchRay(i, positions[i], startPosition, i * 0.02f, _errorPosition, candyColor));
                    }

                    await UniTask.WhenAll(fireTasks);
                }

                float delay = positions.Count * 0.02f;
                TimeSpan duration = TimeSpan.FromSeconds(delay + 0.334f);
                await UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _token);

                activeBooster.Explode();
                _breakGridTask.ReleaseGridCell(gridCell);

                using (ListPool<UniTask>.Get(out List<UniTask> boosterTasks))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell boosterGridCell = _gridCellManager.Get(positions[i]);
                        boosterTasks.Add(_activateBoosterTask.ActivateBooster(boosterGridCell, false, false));
                    }

                    await UniTask.WhenAll(boosterTasks);
                }

                _colorfulBoosterTask.RemoveColor(candyColor);
                _breakGridTask.ReleaseGridCell(gridCell);
                gridCell.LockStates = LockStates.None;
            }
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            CandyColor candyColor = gridCell1.CandyColor;
            Vector3Int colorPosition = gridCell1.GridPosition;
            Vector3Int boosterPosition = gridCell2.GridPosition;

            gridCell1.LockStates = LockStates.Preparing;
            gridCell2.LockStates = LockStates.Preparing;

            if (candyColor == CandyColor.None)
            {
                candyColor = gridCell2.CandyColor;
                colorPosition = gridCell2.GridPosition;
                boosterPosition = gridCell1.GridPosition;
            }

            IGridCell boosterCell = _gridCellManager.Get(boosterPosition);
            IGridCell colorBoosterCell = _gridCellManager.Get(colorPosition);

            IBooster booster = boosterCell.BlockItem as IBooster;
            IColorBooster colorBooster = colorBoosterCell.BlockItem as IColorBooster;
            colorBooster.TriggerNextStage(2);

            booster.IsActivated = true;
            await booster.Activate();

            using (ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                PlayEffect();
                positions.AddRange(_colorfulBoosterTask.FindPositionWithColor(candyColor));
                using (ListPool<UniTask>.Get(out List<UniTask> fireTasks))
                {
                    Vector3 startPosition = boosterCell.WorldPosition;
                    for (int i = 0; i < positions.Count; i++)
                    {
                        fireTasks.Add(FireItemCatchRay(i, positions[i], startPosition, i * 0.02f, colorPosition, candyColor));
                    }

                    await UniTask.WhenAll(fireTasks);
                }

                float delay = positions.Count * 0.02f;
                TimeSpan duration = TimeSpan.FromSeconds(delay + 0.334f);
                await UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _token);

                booster.Explode();
                _breakGridTask.ReleaseGridCell(boosterCell);
                _colorfulBoosterTask.RemoveColor(candyColor);

                using (ListPool<UniTask>.Get(out List<UniTask> boosterTasks))
                {
                    for (int i = 0; i < positions.Count; i++)
                    {
                        IGridCell gridCell = _gridCellManager.Get(positions[i]);
                        boosterTasks.Add(_activateBoosterTask.ActivateBooster(gridCell, false, false));
                    }

                    boosterTasks.Add(_activateBoosterTask.ActivateBooster(colorBoosterCell, false, false));
                    await UniTask.WhenAll(boosterTasks);
                }

                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);

                gridCell1.LockStates = LockStates.None;
                gridCell2.LockStates = LockStates.None;
            }
        }

        private async UniTask FireItemCatchRay(int index, Vector3Int targetPosition, Vector3 position, float delay, Vector3Int originalColorPosition, CandyColor candyColor)
        {
            IGridCell targetGridCell = _gridCellManager.Get(targetPosition);
            ColorfulFireray fireray = SimplePool.Spawn(_colorfulFireray, EffectContainer.Transform
                                                       , Vector3.zero, Quaternion.identity);
            fireray.SetPhaseStep(index);
            await fireray.Fire(targetGridCell, position, delay);
            AddBooster(targetPosition, originalColorPosition, candyColor);
        }

        private void AddBooster(Vector3Int checkPosition, Vector3Int originalColorPosition, CandyColor candyColor)
        {
            float rand = Random.value;
            BoosterType boosterType = rand >= 0.5f ? BoosterType.Horizontal
                                                        : BoosterType.Vertical;
            ItemType itemType = _itemManager.GetItemTypeFromColorAndBoosterType(candyColor, boosterType);
            byte[] boosterProperty = new byte[] { (byte)candyColor, (byte)boosterType, 0, 0 };

            if (originalColorPosition == checkPosition)
                return;

            IGridCell gridCell = _gridCellManager.Get(checkPosition);

            if (gridCell is not IBooster)
            {
                _breakGridTask.ReleaseGridCell(gridCell);
                int state = NumericUtils.BytesToInt(boosterProperty);
                
                _itemManager.Add(new BlockItemPosition
                {
                    Position = checkPosition,
                    ItemData = new BlockItemData
                    {
                        HealthPoint = 1,
                        ItemType = itemType,
                        ItemColor = candyColor,
                        PrimaryState = state
                    }
                });
            }

            IBlockItem blockItem = gridCell.BlockItem;
            if (blockItem is IColorBooster colorBooster && blockItem is IItemEffect effect)
            {
                colorBooster.TriggerNextStage(2);
                effect.PlayStartEffect();
            }
        }

        private void PlayEffect()
        {
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ColorfulCastItems);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ColorfulFirerayCluster);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
