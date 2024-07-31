using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.GridCells
{
    public class GridCellManager : IDisposable
    {
        private Dictionary<Vector3Int, IGridCell> _kpv;

        public Func<Vector3Int, Vector3> ConvertGridToWorldFunction { get; }
        public Func<Vector3, Vector3Int> ConvertWorldToGridFunction { get; }

        public GridCellManager(Func<Vector3Int, Vector3> convertGridToWorldFunction, Func<Vector3, Vector3Int> convertWorldToGridFunction)
        {
            _kpv = new();

            ConvertGridToWorldFunction = convertGridToWorldFunction;
            ConvertWorldToGridFunction = convertWorldToGridFunction;
        }

        public IGridCell Get(Vector3Int position)
        {
            return _kpv.TryGetValue(position, out IGridCell gridCell) ? gridCell : null;
        }

        public IGridCell Add(IGridCell gridCell)
        {
            if (!_kpv.ContainsKey(gridCell.GridPosition))
                _kpv.Add(gridCell.GridPosition, gridCell);
            else
                _kpv[gridCell.GridPosition] = gridCell;

            Vector3 worldPosition = ConvertGridToWorldFunction.Invoke(gridCell.GridPosition);
            
            _kpv[gridCell.GridPosition].WorldPosition = worldPosition;
            _kpv[gridCell.GridPosition].SetGridCellViewPosition();
            
            return _kpv[gridCell.GridPosition];
        }

        public void ReleaseGrid(Vector3Int position)
        {
            if (_kpv.ContainsKey(position))
            {
                IGridCell gridCell = _kpv[position];
                gridCell.ReleaseGrid();
                _kpv[position] = null;
            }
        }

        public void Dispose()
        {
            _kpv.Clear();
        }
    }
}
