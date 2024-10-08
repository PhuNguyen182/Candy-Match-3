using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using GlobalScripts.Probabilities;
using GlobalScripts.Extensions;
using Random = UnityEngine.Random;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FillBoardTask : IDisposable
    {
        private readonly Tilemap _boardTilemap;
        private readonly TileDatabase _tileDatabase;
        private readonly ItemManager _itemManager;
        private readonly GridCellManager _gridCellManager;

        private List<CandyColor> _candyColors;
        private List<float> _colorDistributes;
        private List<ColorFillBlockData> _boardFillRule;
        private List<ColorFillBlockData> _ruledRandomFill;

        public FillBoardTask(GridCellManager gridCellManager, Tilemap boardTilemap, TileDatabase tileDatabase, ItemManager itemManager)
        {
            _gridCellManager = gridCellManager;
            _boardTilemap = boardTilemap;
            _itemManager = itemManager;
            _tileDatabase = tileDatabase;

            _candyColors = new();
            _colorDistributes = new();
            _boardFillRule = new();
            _ruledRandomFill = new();
        }

        private void SetRandomColorDistribute()
        {
            List<int> colorRatios = new();

            for (int i = 0; i < _boardFillRule.Count; i++)
            {
                _candyColors.Add(_boardFillRule[i].DataValue.Color);
                colorRatios.Add(_boardFillRule[i].DataValue.Coefficient);
            }

            for (int i = 0; i < colorRatios.Count; i++)
            {
                float colorDistribute = DistributeCalculator.GetPercentage(colorRatios[i], colorRatios);
                _colorDistributes.Add(colorDistribute);
            }
        }

        private void ShuffleRuledRandomFill()
        {
            _ruledRandomFill.Shuffle();
        }

        private CandyColor GetRandomColor()
        {
            int randIndex = ProbabilitiesController.GetItemByProbabilityRarity(_colorDistributes);
            return _boardFillRule[randIndex].DataValue.Color;
        }

        private CandyColor GetRuledRandomColor(BlockItemData itemData)
        {
            CandyColor candyColor = CandyColor.None;

            for (int i = 0; i < _ruledRandomFill.Count; i++)
            {
                if (itemData.ID == i + 1)
                    return _ruledRandomFill[i].DataValue.Color;
            }

            return candyColor;
        }

        public void SetBoardFillRule(List<ColorFillBlockData> boardFillRule)
        {
            _boardFillRule = boardFillRule;
            SetRandomColorDistribute();
        }

        public void SetRuledRandomFill(List<ColorFillBlockData> ruledRandom)
        {
            _ruledRandomFill = ruledRandom;
            ShuffleRuledRandomFill();
        }

        public void BuildBoard(List<BoardBlockPosition> blockPositions)
        {
            BoardTile boardTile = _tileDatabase.GetBoardTile();

            for (int i = 0; i < blockPositions.Count; i++)
            {
                Vector3Int gridPosition = blockPositions[i].Position;
                _boardTilemap.SetTile(gridPosition, boardTile);
            }
        }

        public void BuildShuffle(List<Vector3Int> positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3Int position = positions[i];
                CandyColor randomColor = GetRandomColorItem(position);
                ItemType itemType = _itemManager.GetItemTypeFromColor(randomColor);
                IGridCell gridCell = _gridCellManager.Get(positions[i]);
                
                if (gridCell.BlockItem is IItemTransform itemTransform)
                    itemTransform.SwitchTo(itemType);
            }
        }

        public void BuildRandom(List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                Vector3Int position = blockPositions[i].Position;
                CandyColor randomColor = GetRandomColorItem(position);
                ItemType itemType = _itemManager.GetItemTypeFromColor(randomColor);

                BlockItemData itemData = new BlockItemData
                {
                    ID = blockPositions[i].ItemData.ID,
                    HealthPoint = blockPositions[i].ItemData.HealthPoint,
                    ItemColor = randomColor,
                    ItemType = itemType
                };

                BlockItemPosition itemPosition = new()
                {
                    Position = position,
                    ItemData = itemData
                };

                _itemManager.Add(itemPosition);
            }
        }

        public void BuildRuledRandom(List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                CandyColor candyColor = GetRuledRandomColor(blockPositions[i].ItemData);
                ItemType itemType = _itemManager.GetItemTypeFromColor(candyColor);

                BlockItemData itemData = new BlockItemData
                {
                    ID = 0,
                    HealthPoint = blockPositions[i].ItemData.HealthPoint,
                    ItemColor = candyColor,
                    ItemType = itemType
                };

                BlockItemPosition itemPosition = new()
                {
                    Position = blockPositions[i].Position,
                    ItemData = itemData
                };

                _itemManager.Add(itemPosition);
            }
        }

        private CandyColor GetRandomColorItem(Vector3Int position)
        {
            using(HashSetPool<CandyColor>.Get(out HashSet<CandyColor> candyColors))
            {
                candyColors.UnionWith(_candyColors);

                IGridCell leftCell1 = _gridCellManager.Get(position + new Vector3Int(-1, 0));
                IGridCell leftCell2 = _gridCellManager.Get(position + new Vector3Int(-2, 0));

                if(leftCell1 != null && leftCell2 != null)
                {
                    if(leftCell1.CandyColor == leftCell2.CandyColor)
                    {
                        candyColors.Remove(leftCell1.CandyColor);
                    }
                }

                IGridCell rightCell1 = _gridCellManager.Get(position + new Vector3Int(1, 0));
                IGridCell rightCell2 = _gridCellManager.Get(position + new Vector3Int(2, 0));

                if (rightCell1 != null && rightCell2 != null)
                {
                    if (rightCell1.CandyColor == rightCell2.CandyColor)
                    {
                        candyColors.Remove(rightCell1.CandyColor);
                    }
                }

                if(leftCell1 != null && rightCell1 != null)
                {
                    if(leftCell1.CandyColor == rightCell1.CandyColor)
                    {
                        candyColors.Remove(leftCell1.CandyColor);
                    }
                }

                IGridCell downCell1 = _gridCellManager.Get(position + new Vector3Int(0, -1));
                IGridCell downCell2 = _gridCellManager.Get(position + new Vector3Int(0, -2));

                if (downCell1 != null && downCell2 != null)
                {
                    if (downCell1.CandyColor == downCell2.CandyColor)
                    {
                        candyColors.Remove(downCell1.CandyColor);
                    }
                }

                IGridCell upCell1 = _gridCellManager.Get(position + new Vector3Int(0, 1));
                IGridCell upCell2 = _gridCellManager.Get(position + new Vector3Int(0, 2));

                if (upCell1 != null && upCell2 != null)
                {
                    if (upCell1.CandyColor == upCell2.CandyColor)
                    {
                        candyColors.Remove(upCell1.CandyColor);
                    }
                }

                if (upCell1 != null && downCell1 != null)
                {
                    if (upCell1.CandyColor == downCell1.CandyColor)
                    {
                        candyColors.Remove(upCell1.CandyColor);
                    }
                }

                CandyColor randomColor;

                if (candyColors.Count == _candyColors.Count)
                    randomColor = GetRandomColor();

                else
                {
                    int randIndex = Random.Range(0, candyColors.Count);
                    randomColor = candyColors.ElementAt(randIndex);
                }

                return randomColor;
            }
        }

        public void Dispose()
        {
            _candyColors.Clear();
            _colorDistributes.Clear();
            _boardFillRule.Clear();
            _ruledRandomFill.Clear();
        }
    }
}
