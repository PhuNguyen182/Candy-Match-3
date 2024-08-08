using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GameInput;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class InputProcessor : IDisposable
    {
        private readonly BoardInput _boardInput;
        private readonly GridCellManager _gridCellManager;

        private LayerMask _gridCellLayer;
        private IDisposable _disposable;

        public InputProcessor(BoardInput boardInput, GridCellManager gridCellManager)
        {
            _boardInput = boardInput;
            _gridCellManager = gridCellManager;
            _gridCellLayer = _boardInput.GridCellLayer;

            DisposableBuilder builder = Disposable.CreateBuilder();
            Observable.EveryUpdate().Subscribe(_ => OnUpdate()).AddTo(ref builder);
            _disposable = builder.Build();
        }

        private void OnUpdate()
        {
            if (_boardInput.IsPress)
            {
                Vector3 inputPosition = _boardInput.WorldMoudePosition;
                Collider2D gridCollider = Physics2D.OverlapPoint(inputPosition, _gridCellLayer);
                
                if (gridCollider != null)
                {
                    if(gridCollider.TryGetComponent<GridCellView>(out var gridCellView))
                    {
                        IGridCell gridCell = _gridCellManager.Get(gridCellView.GridPosition);
                        if(gridCell != null)
                        {
                            if (gridCell.HasItem)
                            {
                                if (gridCell.BlockItem is IItemAnimation animation)
                                    animation.BounceOnTap();
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
