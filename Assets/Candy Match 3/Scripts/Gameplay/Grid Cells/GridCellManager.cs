using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.Gameplay.GridCells
{
    public class GridCellManager : IDisposable
    {
        private BoundsInt _boardActiveBounds;
        private Dictionary<Vector3Int, IGridCell> _kpv;

        public int BoardWidth => MaxPosition.x - MinPosition.x;
        public int BoardHeight => _boardActiveBounds.yMax - _boardActiveBounds.yMin;

        public Vector3Int MinPosition => _boardActiveBounds.min;
        public Vector3Int MaxPosition => _boardActiveBounds.max - Vector3Int.one;

        public Func<Vector3Int, Vector3> ConvertGridToWorldFunction { get; }
        public Func<Vector3, Vector3Int> ConvertWorldToGridFunction { get; }

        public GridCellManager(Func<Vector3Int, Vector3> convertGridToWorldFunction, Func<Vector3, Vector3Int> convertWorldToGridFunction)
        {
            _kpv = new();

            ConvertGridToWorldFunction = convertGridToWorldFunction;
            ConvertWorldToGridFunction = convertWorldToGridFunction;
        }

        public BoundsInt GetActiveBounds()
        {
            return _boardActiveBounds;
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
            
            _kpv[gridCell.GridPosition].SetWorldPosition(worldPosition);
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

        public void SetBoardActiveArea(Tilemap boardArea)
        {
            boardArea.CompressBounds();
            _boardActiveBounds = boardArea.cellBounds;
        }

        public IEnumerable<Vector3Int> Iterator()
        {
            foreach (Vector3Int position in _kpv.Keys)
            {
                yield return position;
            }
        }

        public void ForEach(Action<Vector3Int> callback)
        {
            foreach (Vector3Int position in _kpv.Keys)
            {
                callback.Invoke(position);
            }
        }

        public void Dispose()
        {
            _kpv.Clear();
        }
    }
}
