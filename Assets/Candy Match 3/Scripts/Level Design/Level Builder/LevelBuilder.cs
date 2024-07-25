#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.Gameplay.Models;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private GridInformation gridInformation;

        [Header("Level Rules")]
        [SerializeField] private int targetMove = 0;
        [Header("Score Model")]
        [SerializeField] private ScoreRule scoreRule;
        [Header("Targets")]
        [SerializeField] private List<TargetModel> targetModels;

        [Header("Board Fill Rules")]
        [Tooltip("This rules are used to define how random blocks are filled in the level board via their probabilities")]
        [SerializeField] private List<ColorFillData> boardFillRules;

        [Header("Ruled Random Fills")]
        [Tooltip("This rules are used to define how ruled random colors are filled in the level board via their probabilities")]
        [SerializeField] private List<ColorFillData> ruledRandomFills;

        [Header("Spawner Rules")]
        [Tooltip("This rules are used to define how spawners generate new items in the level board via their probabilities")]
        [SerializeField] private List<SpawnRule> spawnerRules;

        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private Tilemap itemTilemap;
        [SerializeField] private Tilemap spawnerTilemap;
        [SerializeField] private Tilemap statefulTilemap;

        private LevelExporter _levelExporter;
        private LevelImporter _levelImporter;

        [HorizontalGroup(GroupID = "Map Clear 1")]
        [Button]
        private void ClearEntities()
        {
            itemTilemap.ClearAllTiles();
        }

        [HorizontalGroup(GroupID = "Map Clear 1")]
        [Button]
        private void ClearStateful()
        {
            statefulTilemap.ClearAllTiles();
        }

        [HorizontalGroup(GroupID = "Map Clear 2")]
        [Button]
        private void ClearSpawners()
        {
            spawnerTilemap.ClearAllTiles();
        }

        [HorizontalGroup(GroupID = "Map Clear 2")]
        [Button]
        private void ClearBoard()
        {
            boardTilemap.ClearAllTiles();
        }

        [Button]
        private void ClearAllTilemap()
        {
            boardTilemap.ClearAllTiles();
            itemTilemap.ClearAllTiles();
            statefulTilemap.ClearAllTiles();
        }

        [Button]
        private void ScanAllTilemaps()
        {
            boardTilemap.CompressBounds();
            itemTilemap.CompressBounds();
            statefulTilemap.CompressBounds();
        }

        [HorizontalGroup(GroupID = "Level Builder")]
        [Button(Style = ButtonStyle.Box)]
        public void Export(int level, bool writeToFile = true)
        {
            ScanAllTilemaps();
            _levelExporter = new();
            _levelExporter.ClearModel()
                          .BuildTargetMove(targetMove)
                          .BuildScoreRule(scoreRule)
                          .BuildLevelTarget(targetModels)
                          .BuildBoardFill(boardFillRules)
                          .BuildRuledRandomFill(ruledRandomFills)
                          .BuildSpawnRule(spawnerRules)
                          .BuildBoard(boardTilemap)
                          .BuildColorItems(itemTilemap)
                          .BuildSingleItems(itemTilemap)
                          .BuildStateful(statefulTilemap)
                          .BuildSpawner(spawnerTilemap)
                          .Export(level, writeToFile);
        }

        [HorizontalGroup(GroupID = "Level Builder")]
        [Button(Style = ButtonStyle.Box)]
        private void Import(int level, bool readFromFile = true)
        {
            if (readFromFile)
            {
                string levelData;
                string levelName = $"level_{level}";
                string folder = LevelFolderClassifyer.GetLevelRangeFolderName(level);
                string levelPath = $"Assets/Candy Match 3/Level Data/{folder}/{levelName}.txt";

                using (StreamReader streamReader = new StreamReader(levelPath))
                {
                    levelData = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                LevelModel levelModel;
                using (StringReader stringReader = new(levelData))
                {
                    using (JsonReader jsonReader = new JsonTextReader(stringReader))
                    {
                        JsonSerializer jsonSerializer = new();
                        levelModel = jsonSerializer.Deserialize<LevelModel>(jsonReader);
                        jsonReader.Close();
                    }

                    stringReader.Close();
                }

                _levelImporter = new(tileDatabase);
                _levelImporter.BuildTargetMove(levelModel.TargetMove, out targetMove)
                              .BuildScoreRule(levelModel.ScoreRule, out scoreRule)
                              .BuildSpawnRule(levelModel.SpawnerRules, out spawnerRules)
                              .BuildBoardFill(levelModel.BoardFillRule, out boardFillRules)
                              .BuildRuledRandomFill(levelModel.RuledRandomFill, out ruledRandomFills)
                              .BuildLevelTarget(levelModel.LevelTargetData, out targetModels)
                              .BuildBoard(boardTilemap, levelModel.BoardBlockPositions)
                              .BuildColorTiles(itemTilemap, levelModel.ColorBlockItemPositions)
                              .BuildColorBoosterTiles(itemTilemap, levelModel.ColorBoosterItemPositions)
                              .BuildRandomTiles(itemTilemap, levelModel.RandomBlockItemPositions)
                              .BuildRuledRandom(itemTilemap, levelModel.RuledRandomBlockPositions)
                              .BuildBreakable(itemTilemap, levelModel.BreakableItemPositions)
                              .BuildUnbreakable(itemTilemap, levelModel.UnbreakableItemPositions)
                              .BuildBooster(itemTilemap, levelModel.BoosterItemPositions)
                              .BuildCollectible(itemTilemap, levelModel.CollectibleItemPositions)
                              .BuildStateful(statefulTilemap, levelModel.StatefulBlockPositions)
                              .BuildSpawner(spawnerTilemap, levelModel.SpawnerBlockPositions)
                              .Import();
            }
        }
    }
}
#endif
