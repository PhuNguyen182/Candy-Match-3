using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
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

        public ColorfulStripedBoosterTask(ItemManager itemManager, GridCellManager gridCellManager, BreakGridTask breakGridTask, ActivateBoosterTask activateBoosterTask)
        {
            _itemManager = itemManager;
            _gridCellManager = gridCellManager;
            _breakGridTask = breakGridTask;
            _activateBoosterTask = activateBoosterTask;
            _colorfulBoosterTask = _activateBoosterTask.ColorfulBoosterTask;
        }

        public async UniTask Activate(IGridCell gridCell1, IGridCell gridCell2)
        {
            CandyColor candyColor = gridCell1.CandyColor;
            Vector3Int colorPosition = gridCell1.GridPosition;

            if (candyColor == CandyColor.None)
            {
                candyColor = gridCell2.CandyColor;
                colorPosition = gridCell2.GridPosition;
            }

            using (var listPool = ListPool<Vector3Int>.Get(out List<Vector3Int> positions))
            {
                positions.AddRange(_colorfulBoosterTask.FindPositionWithColor(candyColor));

                for (int i = 0; i < positions.Count; i++)
                {
                    float rand = Random.value;
                    ColorBoosterType boosterType = rand >= 0.5f ? ColorBoosterType.Horizontal
                                                                : ColorBoosterType.Vertical;
                    ItemType itemType = _itemManager.GetItemTypeFromColorAndBoosterType(candyColor, boosterType);
                    byte[] boosterProperty = new byte[] { (byte)candyColor, (byte)boosterType, 0, 0 };

                    if (colorPosition == positions[i])
                        continue;

                    IGridCell gridCell = _gridCellManager.Get(positions[i]);
                    if (gridCell.BlockItem is IBreakable breakable)
                    {
                        if (breakable.Break())
                            _breakGridTask.ReleaseGridCell(gridCell);
                    }

                    _itemManager.Add(new BlockItemPosition
                    {
                        Position = positions[i],
                        ItemData = new BlockItemData
                        {
                            ID = 0,
                            HealthPoint = 1,
                            ItemType = itemType,
                            ItemColor = candyColor,
                            PrimaryState = NumericUtils.BytesToInt(boosterProperty)
                        }
                    });
                }

                for (int i = 0; i < positions.Count; i++)
                {
                    IGridCell gridCell = _gridCellManager.Get(positions[i]);
                    await _activateBoosterTask.ActivateBooster(gridCell);
                }

                _breakGridTask.ReleaseGridCell(gridCell1);
                _breakGridTask.ReleaseGridCell(gridCell2);
            }
        }

        public void Dispose()
        {

        }
    }
}
