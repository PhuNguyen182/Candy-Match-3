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

        private CandyColor GetRandomColor()
        {
            int randIndex = ProbabilitiesController.GetItemByProbabilityRarity(_colorDistributes);
            return _boardFillRule[randIndex].DataValue.Color;
        }

        public void SetBoardFillRule(List<ColorFillBlockData> boardFillRule)
        {
            _boardFillRule = boardFillRule;
            SetRandomColorDistribute();
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

        public void Dispose()
        {
            _colorDistributes.Clear();
            _boardFillRule.Clear();
        }
    }
}
