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
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.Statefuls;

namespace CandyMatch3.Scripts.Gameplay.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private Tilemap boardTilemap;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GridCellView gridCellViewPrefab;

        private ItemFactory _itemFactory;
        private ItemManager _itemManager;
        private MetaItemManager _metaItemManager;
        private GridCellManager _gridCellManager;
        private FillBoardTask _fillBoardTask;
        private BreakGridTask _breakGridTask;
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                _gameTaskManager.CheckMoveOnStart();
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

            _fillBoardTask = new(boardTilemap, tileDatabase, _itemManager, _metaItemManager);
            _fillBoardTask.AddTo(ref builder);

            _gameTaskManager = new(_gridCellManager);
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
                _gridCellManager.Add(gridCell);
            }

            _fillBoardTask.SetBoardFillRule(levelModel.BoardFillRule);
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

            //_gameTaskManager.CheckMoveOnStart();
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
