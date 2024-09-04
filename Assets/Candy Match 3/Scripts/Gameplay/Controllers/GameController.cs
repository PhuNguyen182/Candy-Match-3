using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Common.Factories;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.Gameplay.GameTasks;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Controllers
{
    public class GameController : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;

        [Header("Databases")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private MiscCollection miscCollection;
        [SerializeField] private StatefulSpriteDatabase statefulSpriteDatabase;

        [Header("Containers")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform miscContainer;

        [Header("Board Utils")]
        [SerializeField] private GridCellView gridCellViewPrefab;
        [SerializeField] private BoardInput boardInput;

        private ItemManager _itemManager;
        private FactoryManager _factoryManager;
        private MetaItemManager _metaItemManager;
        private GridCellManager _gridCellManager;
        private FillBoardTask _fillBoardTask;
        private BreakGridTask _breakGridTask;
        private MatchItemsTask _matchItemsTask;
        private SpawnItemTask _spawnItemTask;        
        private GameTaskManager _gameTaskManager;

        private int _check = 0;
        private CancellationToken _destroyToken;

        private void Awake()
        {
            Setup();
            Initialize();
        }

        private void Start()
        {
            if(PlayGameConfig.Current != null)
            {
                LevelModel levelModel = PlayGameConfig.Current.LevelModel;
                GenerateLevel(levelModel);
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                _check = _check + 1;

                if (_check % 4 == 0)
                    Time.timeScale = 1;
                else if (_check % 4 == 1)
                    Time.timeScale = 0.1f;
                else if (_check % 4 == 2)
                    Time.timeScale = 0.05f;
                else
                    Time.timeScale = 0.02f;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Suggest(true);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Suggest(false);
            }
        }
#endif

        private void Setup()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
#if !UNITY_EDITOR
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
#endif
        }

        private void Initialize()
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = new(ConvertGridToWorld, ConvertWorldToGrid);
            _gridCellManager.AddTo(ref builder);

            _metaItemManager = new();
            _factoryManager = new(_gridCellManager, statefulSpriteDatabase, itemDatabase, itemContainer);
            _itemManager = new(_gridCellManager, _metaItemManager, _factoryManager.ItemFactory);

            _fillBoardTask = new(_gridCellManager, boardTilemap, tileDatabase, _itemManager);
            _fillBoardTask.AddTo(ref builder);

            _breakGridTask = new(_gridCellManager, _metaItemManager, _itemManager);
            _breakGridTask.AddTo(ref builder);

            _matchItemsTask = new(_gridCellManager, _breakGridTask);
            _matchItemsTask.AddTo(ref builder);

            _spawnItemTask = new(_gridCellManager, _itemManager);
            _spawnItemTask.AddTo(ref builder);

            _gameTaskManager = new(boardInput, _gridCellManager, _itemManager, _spawnItemTask
                                   , _matchItemsTask, _metaItemManager, _breakGridTask, effectDatabase);
            _gameTaskManager.AddTo(ref builder);

            builder.RegisterTo(_destroyToken);
        }

        private void GenerateLevel(LevelModel levelModel)
        {
            for (int i = 0; i < levelModel.BoardBlockPositions.Count; i++)
            {
                Vector3Int gridPosition = levelModel.BoardBlockPositions[i].Position;
                GridCellView gridCellView = Instantiate(gridCellViewPrefab, gridContainer);

                GridCell gridCell = new();
                gridCell.GridPosition = gridPosition;

                IGridStateful gridStateful = new AvailableState();
                gridCell.SetGridStateful(gridStateful);
                gridCell.SetGridCellView(gridCellView);
                gridCell.SetGridID(gridCellView.gameObject.GetInstanceID());
                _gridCellManager.Add(gridCell);
            }

            _fillBoardTask.SetBoardFillRule(levelModel.BoardFillRule);
            _fillBoardTask.SetRuledRandomFill(levelModel.RuledRandomFill);
            _fillBoardTask.BuildBoard(levelModel.BoardBlockPositions);
            _gridCellManager.SetBoardActiveArea(boardTilemap);

            for (int i = 0; i < levelModel.ColorBlockItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.ColorBlockItemPositions[i]);
            }

            for (int i = 0; i < levelModel.ColorBoosterItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.ColorBoosterItemPositions[i]);
            }

            for (int i = 0; i < levelModel.BoosterItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.BoosterItemPositions[i]);
            }

            for (int i = 0; i < levelModel.BreakableItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.BreakableItemPositions[i]);
            }

            for (int i = 0; i < levelModel.UnbreakableItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.UnbreakableItemPositions[i]);
            }

            for (int i = 0; i < levelModel.CollectibleItemPositions.Count; i++)
            {
                _itemManager.Add(levelModel.CollectibleItemPositions[i]);
            }

            for (int i = 0; i < levelModel.StatefulBlockPositions.Count; i++)
            {
                var stateful = levelModel.StatefulBlockPositions[i];
                IGridStateful gridStateful = _factoryManager.ProduceStateful(stateful);
                IGridCell gridCell = _gridCellManager.Get(stateful.Position);
                gridCell.SetGridStateful(gridStateful);
            }

            for (int i = 0; i < levelModel.SpawnerBlockPositions.Count; i++)
            {
                IGridCell gridCell = _gridCellManager.Get(levelModel.SpawnerBlockPositions[i].Position);
                gridCell.IsSpawner = true;

                _spawnItemTask.AddSpawnerPosition(levelModel.SpawnerBlockPositions[i]);
                Vector3 spawnerPosition = ConvertGridToWorld(levelModel.SpawnerBlockPositions[i].Position + Vector3Int.up);
                Instantiate(miscCollection.SpawnerMask, spawnerPosition, Quaternion.identity, miscContainer);
            }

            for (int i = 0; i < levelModel.CollectibleCheckBlockPositions.Count; i++)
            {
                IGridCell gridCell = _gridCellManager.Get(levelModel.CollectibleCheckBlockPositions[i].Position);
                gridCell.IsCollectible = true;

                Vector3 checkPosition = ConvertGridToWorld(levelModel.CollectibleCheckBlockPositions[i].Position);
                Instantiate(miscCollection.CollectibleCheckSign, checkPosition, Quaternion.identity, miscContainer);
            }

            // Build ruled random first, then build random later
            _fillBoardTask.BuildRuledRandom(levelModel.RuledRandomBlockPositions);
            _fillBoardTask.BuildRandom(levelModel.RandomBlockItemPositions);

            _spawnItemTask.SetItemSpawnerData(levelModel.SpawnerRules);
            _gameTaskManager.BuildSuggest();
            _gameTaskManager.SetInputActive(true);
        }

        public Vector3 ConvertGridToWorld(Vector3Int position)
        {
            return boardTilemap.GetCellCenterWorld(position);
        }

        public Vector3Int ConvertWorldToGrid(Vector3 position)
        {
            return boardTilemap.WorldToCell(position);
        }

        private void Suggest(bool isSuggest)
        {
            _gameTaskManager.Suggest(isSuggest);
        }

        private void OnDestroy()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}
