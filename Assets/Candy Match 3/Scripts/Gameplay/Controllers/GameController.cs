using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Cinemachine;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Common.Factories;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster;
using CandyMatch3.Scripts.Gameplay.GameTasks.SpecialItemTasks;
using CandyMatch3.Scripts.Gameplay.GameUI.EndScreen;
using CandyMatch3.Scripts.Gameplay.GameTasks;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private MainGamePanel mainGamePanel;
        [SerializeField] private EndGameScreen endGameScreen;
        [SerializeField] private InGameBoosterPanel inGameBoosterPanel;
        [SerializeField] private InGameSettingPanel settingSidePanel;

        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;

        [Header("Databases")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private TargetDatabase targetDatabase;
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private MiscCollection miscCollection;
        [SerializeField] private InGameBoosterPackDatabase inGameBoosterPackDatabase;
        [SerializeField] private StatefulSpriteDatabase statefulSpriteDatabase;

        [Header("Containers")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform miscContainer;

        [Header("Board Utils")]
        [SerializeField] private GridCellView gridCellViewPrefab;
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private BoardInput boardInput;

        private ItemManager _itemManager;
        private ComplimentTask _complimentTask;
        private FactoryManager _factoryManager;
        private MetaItemManager _metaItemManager;
        private GridCellManager _gridCellManager;
        private FillBoardTask _fillBoardTask;
        private BreakGridTask _breakGridTask;
        private MatchItemsTask _matchItemsTask;
        private SpawnItemTask _spawnItemTask;
        private SpecialItemTask _specialItemTasks;
        private CameraShakeTask _cameraShakeTask;
        private GameTaskManager _gameTaskManager;
        private MessageBrokerController _messageBrokerController;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
                _gameTaskManager.Test();
        }

        private void Setup()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        private void Initialize()
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _messageBrokerController = new();
            _gridCellManager = new(ConvertGridToWorld, ConvertWorldToGrid);
            _gridCellManager.AddTo(ref builder);

            _metaItemManager = new();
            _factoryManager = new(_gridCellManager, statefulSpriteDatabase, itemDatabase, itemContainer);
            _itemManager = new(_gridCellManager, _metaItemManager, _factoryManager.ItemFactory);

            _fillBoardTask = new(_gridCellManager, boardTilemap, tileDatabase, _itemManager);
            _fillBoardTask.AddTo(ref builder);

            _breakGridTask = new(_gridCellManager, _metaItemManager, _itemManager);
            _breakGridTask.AddTo(ref builder);

            _specialItemTasks = new(_gridCellManager, _itemManager, _breakGridTask);
            _specialItemTasks.AddTo(ref builder);

            _matchItemsTask = new(_gridCellManager, _breakGridTask);
            _matchItemsTask.AddTo(ref builder);

            _spawnItemTask = new(_gridCellManager, _itemManager);
            _spawnItemTask.AddTo(ref builder);

            _complimentTask = new(mainGamePanel.CharacterEmotion);
            _complimentTask.AddTo(ref builder);

            _cameraShakeTask = new(impulseSource);
            _cameraShakeTask.AddTo(ref builder);

            _gameTaskManager = new(boardInput, _gridCellManager, _itemManager, _spawnItemTask, _matchItemsTask, _metaItemManager
                                  , _breakGridTask, effectDatabase, mainGamePanel, endGameScreen, settingSidePanel, targetDatabase
                                  , inGameBoosterPanel, inGameBoosterPackDatabase, _specialItemTasks, _complimentTask, _fillBoardTask);
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

            _gameTaskManager.BuildShufflePositions();
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
                AddBreakableItem(levelModel.BreakableItemPositions[i]);
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
            _gameTaskManager.ShuffleBoard();
            _gameTaskManager.InitInGameBooster();
            _gameTaskManager.BuildTarget(levelModel);
            _gameTaskManager.BuildBoardMovementCheck();
            _gameTaskManager.StartGame(levelModel);
        }

        private void AddBreakableItem(BlockItemPosition blockItemPosition)
        {
            _itemManager.Add(blockItemPosition);

            if (blockItemPosition.ItemData.ItemType == ItemType.Chocolate)
                _specialItemTasks.ExpandableItemTask.AddExpandablePosition(blockItemPosition.Position);
        }

        public Vector3 ConvertGridToWorld(Vector3Int position)
        {
            return boardTilemap.GetCellCenterWorld(position);
        }

        public Vector3Int ConvertWorldToGrid(Vector3 position)
        {
            return boardTilemap.WorldToCell(position);
        }

        private void OnDestroy()
        {
            GC.Collect();
        }
    }
}
