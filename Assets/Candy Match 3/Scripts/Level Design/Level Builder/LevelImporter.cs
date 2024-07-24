using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.TopTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles;
using CandyMatch3.Scripts.Common.Enums;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelImporter
    {
        private readonly TileDatabase _tileDatabase;

        public LevelImporter(TileDatabase tileDatabase)
        {
            _tileDatabase = tileDatabase;
        }

        public LevelImporter BuildTargetMove(int savedTargetMove, out int targetMove)
        {
            targetMove = savedTargetMove;
            return this;
        }

        public LevelImporter BuildScoreRule(ScoreRule savedScoreRule, out ScoreRule scoreRule)
        {
            scoreRule = savedScoreRule;
            return this;
        }

        public LevelImporter BuildLevelTarget(List<LevelTargetData> levelTargetDatas, out List<TargetModel> targetModels)
        {
            List<TargetModel> targets = new();

            for (int i = 0; i < levelTargetDatas.Count; i++)
            {
                targets.Add(new TargetModel
                {
                    TargetAmount = levelTargetDatas[i].DataValue.TargetAmount,
                    Target = levelTargetDatas[i].DataValue.Target
                });
            }

            targetModels = targets;
            return this;
        }

        public LevelImporter BuildSpawnRule(List<SpawnRule> savedSpawnRules, out List<SpawnRule> spawnRules)
        {
            spawnRules = savedSpawnRules;
            return this;
        }

        public LevelImporter BuildBoardFill(List<ColorFillBlockData> colorFills, out List<Gameplay.Models.ColorFillData> colorFillDatas)
        {
            List<Gameplay.Models.ColorFillData> colors = new();

            for (int i = 0; i < colorFills.Count; i++)
            {
                colors.Add(new Gameplay.Models.ColorFillData
                {
                    Coefficient = colorFills[i].DataValue.Coefficient,
                    Color = colorFills[i].DataValue.Color,
                });
            }

            colorFillDatas = colors;
            return this;
        }

        public LevelImporter BuildBoard(Tilemap tilemap, List<BoardBlockPosition> blockPositions)
        {
            tilemap.ClearAllTiles();

            for (int i = 0; i < blockPositions.Count; i++)
            {
                BoardTile boardTile = _tileDatabase.GetBoardTile();
                tilemap.SetTile(blockPositions[i].Position, boardTile);
            }
            
            return this;
        }

        public LevelImporter BuildColorTiles(Tilemap tilemap, List<BlockItemPosition> blockItemPositions)
        {
            tilemap.ClearAllTiles();

            for (int i = 0; i < blockItemPositions.Count; i++)
            {
                int id = blockItemPositions[i].ItemData.ID;
                ItemType itemType = blockItemPositions[i].ItemData.ItemType;
                ColorItemTile colorItemTile = _tileDatabase.GetColorItemTile(id, itemType);
                tilemap.SetTile(blockItemPositions[i].Position, colorItemTile);
            }

            return this;
        }

        public LevelImporter BuildColorBoosterTiles(Tilemap tilemap, List<BlockItemPosition> blockItemPositions)
        {
            for (int i = 0; i < blockItemPositions.Count; i++)
            {
                int id = blockItemPositions[i].ItemData.ID;
                byte[] boosterState = NumericUtils.IntToBytes(blockItemPositions[i].ItemData.PrimaryState);
                CandyColor candyColor = (CandyColor)boosterState[0];
                ColorBoosterType colorBoosterType = (ColorBoosterType)boosterState[1];
                ColorBoosterTile colorBoosterTile = _tileDatabase.GetColorBoosterTile(id, candyColor, colorBoosterType);
                tilemap.SetTile(blockItemPositions[i].Position, colorBoosterTile);
            }

            return this;
        }

        public LevelImporter BuildRandomTiles(Tilemap tilemap, List<BlockItemPosition> blockItemPositions)
        {
            for (int i = 0; i < blockItemPositions.Count; i++)
            {
                RandomTile colorItemTile = _tileDatabase.GetRandomTile();
                tilemap.SetTile(blockItemPositions[i].Position, colorItemTile);
            }

            return this;
        }

        public LevelImporter BuildBreakable(Tilemap tilemap, List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                int id = blockPositions[i].ItemData.ID;
                int healthPoint = blockPositions[i].ItemData.HealthPoint;
                ItemType itemType = blockPositions[i].ItemData.ItemType;
                SingleBreakableTile breakableTile = _tileDatabase.GetBreakableTile(id, itemType, healthPoint);
                tilemap.SetTile(blockPositions[i].Position, breakableTile);
            }

            return this;
        }

        public LevelImporter BuildUnbreakable(Tilemap tilemap, List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                UnbreakableTile unbreakableTile = _tileDatabase.GetUnbreakableTile();
                tilemap.SetTile(blockPositions[i].Position, unbreakableTile);
            }

            return this;
        }

        public LevelImporter BuildBooster(Tilemap tilemap, List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                int id = blockPositions[i].ItemData.ID;
                ItemType itemType = blockPositions[i].ItemData.ItemType;
                BoosterTile boosterTile = _tileDatabase.GetBoosterTile(id, itemType);
                tilemap.SetTile(blockPositions[i].Position, boosterTile);
            }

            return this;
        }

        public LevelImporter BuildCollectible(Tilemap tilemap, List<BlockItemPosition> blockPositions)
        {
            for (int i = 0; i < blockPositions.Count; i++)
            {
                int id = blockPositions[i].ItemData.ID;
                CollectibleTile collectibleTile = _tileDatabase.GetCollectibleTile(id, blockPositions[i].ItemData.ItemType);
                tilemap.SetTile(blockPositions[i].Position, collectibleTile);
            }

            return this;
        }

        public LevelImporter BuildStateful(Tilemap tilemap, List<StatefulBlockPosition> blockPositions)
        {
            tilemap.ClearAllTiles();

            for (int i = 0; i < blockPositions.Count; i++)
            {
                int id = blockPositions[i].ItemData.ID;
                int healthPoint = blockPositions[i].ItemData.HealthPoint;
                StatefulGroupType groupType = blockPositions[i].ItemData.GroupType;
                StatefulTile statefulTile = _tileDatabase.GetStatefulTile(id, groupType, healthPoint);
            }

            return this;
        }

        public LevelImporter BuildSpawner(Tilemap tilemap, List<SpawnerBlockPosition> blockPositions)
        {
            tilemap.ClearAllTiles();

            for (int i = 0; i < blockPositions.Count; i++)
            {
                int id = blockPositions[i].ItemData.ID;
                SpawnerTile spawnerTile = _tileDatabase.GetSpawnerTile(id);
                tilemap.SetTile(blockPositions[i].Position, spawnerTile);
            }

            return this;
        }

        public void Import()
        {
            Debug.Log("Import Successfully!");
        }
    }
}
