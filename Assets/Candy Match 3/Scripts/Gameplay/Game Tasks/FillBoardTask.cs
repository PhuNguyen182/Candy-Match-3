using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles;
using CandyMatch3.Scripts.Gameplay.Strategies;
using GlobalScripts.Probabilities;
using GlobalScripts.Extensions;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FillBoardTask : IDisposable
    {
        private readonly Tilemap _boardTilemap;
        private readonly TileDatabase _tileDatabase;
        private readonly ItemManager _itemManager;
        private readonly MetaItemManager _metaItemManager;

        private List<float> _colorDistributes = new();
        private List<ColorFillBlockData> _boardFillRule = new();
        private List<ColorFillBlockData> _ruledRandomFill = new();

        public FillBoardTask(Tilemap boardTilemap, TileDatabase tileDatabase, ItemManager itemManager, MetaItemManager metaItemManager)
        {
            _boardTilemap = boardTilemap;
            _itemManager = itemManager;
            _tileDatabase = tileDatabase;
            _metaItemManager = metaItemManager;
        }

        private void SetRandomColorDistribute()
        {
            List<int> colorRatios = new();

            for (int i = 0; i < _boardFillRule.Count; i++)
            {
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
                if (itemData.ID == i)
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

        public void BuildRandom(List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                CandyColor randomColor = GetRandomColor();
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
                    Position = blockPositions[i].Position,
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

        public void Dispose()
        {
            _colorDistributes.Clear();
            _boardFillRule.Clear();
        }
    }
}
