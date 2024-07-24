using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.TopTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles;
using GlobalScripts.Extensions;
using GlobalScripts.Utils;
using Newtonsoft.Json;
using UnityEditor;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelExporter
    {
        private readonly LevelModel _levelModel;

        public LevelExporter()
        {
            _levelModel = new();
        }

        private bool IsColorTile(SingleItemTile tile)
        {
            if (tile is ColorItemTile)
                return false;

            else if (tile is ColorBoosterTile)
                return false;

            else if (tile is RandomTile)
                return false;

            return true;
        }

        public LevelExporter ClearModel()
        {
            _levelModel.ClearModel();
            return this;
        }

        public LevelExporter BuildTargetMove(int targetMove)
        {
            _levelModel.TargetMove = targetMove;
            return this;
        }

        public LevelExporter BuildScoreRule(ScoreRule scoreRule)
        {
            _levelModel.ScoreRule = scoreRule;
            return this;
        }

        public LevelExporter BuildLevelTarget(List<TargetModel> targetModels)
        {
            for (int i = 0; i < targetModels.Count; i++)
            {
                _levelModel.LevelTargetData.Add(new LevelTargetData
                {
                    DataValue = new TargetData
                    {
                        TargetAmount = targetModels[i].TargetAmount,
                        Target = targetModels[i].Target
                    }
                });
            }

            return this;
        }

        public LevelExporter BuildSpawnRule(List<SpawnRule> spawnRules)
        {
            _levelModel.SpawnerRules = spawnRules;
            return this;
        }

        public LevelExporter BuildBoardFill(List<Gameplay.Models.ColorFillData> colorFillDatas)
        {
            for (int i = 0; i < colorFillDatas.Count; i++)
            {
                _levelModel.BoardFillRule.Add(new ColorFillBlockData
                {
                    DataValue = new Common.CustomData.ColorFillData
                    {
                        Coefficient = colorFillDatas[i].Coefficient,
                        Color = colorFillDatas[i].Color,
                    }
                });
            }

            return this;
        }

        public LevelExporter BuildBoard(Tilemap tilemap)
        {
            var mapPositions = tilemap.cellBounds.Iterator2D();

            foreach (Vector3Int position in mapPositions)
            {
                BoardTile boardTile = tilemap.GetTile<BoardTile>(position);

                if (boardTile == null)
                    continue;

                _levelModel.BoardBlockPositions.Add(new BoardBlockPosition
                {
                    Position = position,
                });
            }

            return this;
        }

        public LevelExporter BuildColorItems(Tilemap tilemap)
        {
            var mapPositions = tilemap.cellBounds.Iterator2D();

            foreach (Vector3Int position in mapPositions)
            {
                ColorItemTile colorItemTile = tilemap.GetTile<ColorItemTile>(position);

                if (colorItemTile != null)
                {
                    _levelModel.ColorBlockItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = colorItemTile.ItemID,
                            ItemType = colorItemTile.ItemType,
                            HealthPoint = 1,
                        }
                    });
                }

                ColorBoosterTile colorBoosterTile = tilemap.GetTile<ColorBoosterTile>(position);

                if(colorBoosterTile != null)
                {
                    byte[] boosterState = new byte[] 
                    { 
                        (byte)colorBoosterTile.CandyColor, 
                        (byte)colorBoosterTile.ColorBoosterType, 
                        0, 0 
                    };

                    _levelModel.ColorBoosterItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = colorBoosterTile.ItemID,
                            ItemType = colorBoosterTile.ItemType,
                            HealthPoint = 1,
                            PrimaryState = NumericUtils.BytesToInt(boosterState)
                        }
                    });
                }

                RandomTile randomTile = tilemap.GetTile<RandomTile>(position);

                if(randomTile != null)
                {
                    _levelModel.RandomBlockItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = randomTile.ItemID,
                            ItemType = ItemType.Random,
                            HealthPoint = 1,
                        }
                    });
                }
            }

            return this;
        }

        public LevelExporter BuildSingleItems(Tilemap tilemap)
        {
            var mapPositions = tilemap.cellBounds.Iterator2D();

            foreach (Vector3Int position in mapPositions)
            {
                SingleItemTile singleItemTile = tilemap.GetTile<SingleItemTile>(position);

                if (singleItemTile == null)
                    continue;

                if (!IsColorTile(singleItemTile))
                    continue;

                if (singleItemTile is SingleBreakableTile breakableTile)
                {
                    _levelModel.BreakableItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = breakableTile.ItemID,
                            ItemType = breakableTile.ItemType,
                            HealthPoint = breakableTile.HealthPoint
                        }
                    });
                }

                else if (singleItemTile is UnbreakableTile unbreakableTile)
                {
                    _levelModel.UnbreakableItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = unbreakableTile.ItemID,
                            ItemType = ItemType.Unbreakable,
                            HealthPoint = 100
                        }
                    });
                }

                else if (singleItemTile is CollectibleTile collectibleTile)
                {
                    _levelModel.CollectibleItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = collectibleTile.ItemID,
                            ItemType = collectibleTile.ItemType,
                            HealthPoint = 100
                        }
                    });
                }

                else if(singleItemTile is BoosterTile boosterTile)
                {
                    _levelModel.BoosterItemPositions.Add(new BlockItemPosition
                    {
                        Position = position,
                        ItemData = new BlockItemData
                        {
                            ID = boosterTile.ItemID,
                            ItemType = boosterTile.ItemType,
                            HealthPoint = 100
                        }
                    });
                }
            }

            return this;
        }

        public LevelExporter BuildStateful(Tilemap tilemap)
        {
            var mapPosition = tilemap.cellBounds.Iterator2D();

            foreach (Vector3Int position in mapPosition)
            {
                StatefulTile statefulTile = tilemap.GetTile<StatefulTile>(position);

                if (statefulTile == null)
                    continue;

                _levelModel.StatefulBlockPositions.Add(new StatefulBlockPosition
                {
                    Position = position,
                    ItemData = new StatefulBlockData
                    {
                        ID = statefulTile.ItemID,
                        GroupType = statefulTile.GroupType,
                        HealthPoint = statefulTile.HealthPoint
                    }
                });
            }

            return this;
        }

        public LevelExporter BuildSpawner(Tilemap tilemap)
        {
            var mapPositions = tilemap.cellBounds.Iterator2D();

            foreach (Vector3Int position in mapPositions)
            {
                SpawnerTile spawnerTile = tilemap.GetTile<SpawnerTile>(position);

                if (spawnerTile == null)
                    continue;

                _levelModel.SpawnerBlockPositions.Add(new SpawnerBlockPosition
                {
                    Position = position,
                    ItemData = new SpawnerBlockData
                    {
                        ID = spawnerTile.ItemID
                    }
                });
            }

            return this;
        }

        public string Export(string level, bool writeToFile = true)
        {
            string levelPath = $"Assets/Candy Match 3/Level Data/{level}.txt";
            string json = JsonConvert.SerializeObject(_levelModel, Formatting.None);

            if (writeToFile)
            {
                using (StreamWriter writer = new(levelPath))
                {
                    writer.Write(json);
                    writer.Close();
                }

#if UNITY_EDITOR
                AssetDatabase.ImportAsset(levelPath);
#endif
            }

            Debug.Log(writeToFile ? json : "Get level data at output placeholder!");
            return json;
        }
    }
}
