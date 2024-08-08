using R3;
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

namespace CandyMatch3.Scripts.Gameplay.Controllers
{
    public class GameController : MonoBehaviour
    {
        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;

        [Header("Databases")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private MiscCollection miscCollection;

        [Header("Containers")]
        [SerializeField] private Transform gridContainer;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private Transform miscContainer;

        [Header("Board Utils")]
        [SerializeField] private GridCellView gridCellViewPrefab;
        [SerializeField] private BoardInput boardInput;

        private ItemFactory _itemFactory;
        private ItemManager _itemManager;
        private MetaItemManager _metaItemManager;
        private GridCellManager _gridCellManager;
        private FillBoardTask _fillBoardTask;
        private BreakGridTask _breakGridTask;
        private SpawnItemTask _spawnItemTask;
        private GameTaskManager _gameTaskManager;

        private void Awake()
        {
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

        private void Initialize()
        {
            DisposableBuilder builder = Disposable.CreateBuilder();

            _gridCellManager = new(ConvertGridToWorld, ConvertWorldToGrid);
            _gridCellManager.AddTo(ref builder);

            _metaItemManager = new();
            _itemFactory = new(itemDatabase, itemContainer);
            _itemManager = new(_gridCellManager, _metaItemManager, _itemFactory);

            _fillBoardTask = new(boardTilemap, tileDatabase, _itemManager);
            _fillBoardTask.AddTo(ref builder);

            _spawnItemTask = new(_gridCellManager, _itemManager);
            _spawnItemTask.AddTo(ref builder);

            _gameTaskManager = new(boardInput, _gridCellManager, _itemManager, _spawnItemTask);
            _gameTaskManager.AddTo(ref builder);

            builder.RegisterTo(this.destroyCancellationToken);
        }

        private void GenerateLevel(LevelModel levelModel)
        {
            for (int i = 0; i < levelModel.BoardBlockPositions.Count; i++)
            {
                GridCell gridCell;

                Vector3Int gridPosition = levelModel.BoardBlockPositions[i].Position;
                GridCellView gridCellView = Instantiate(gridCellViewPrefab, gridContainer);
                
                gridCell = new();
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

            _fillBoardTask.BuildRandom(levelModel.RandomBlockItemPositions);
            _fillBoardTask.BuildRuledRandom(levelModel.RuledRandomBlockPositions);

            _spawnItemTask.SetItemSpawnerData(levelModel.SpawnerRules);
            _gameTaskManager.CheckMoveOnStart();
        }

        public Vector3 ConvertGridToWorld(Vector3Int position)
        {
            return boardTilemap.GetCellCenterWorld(position);
        }

        public Vector3Int ConvertWorldToGrid(Vector3 position)
        {
            return boardTilemap.WorldToCell(position);
        }
    }
}
