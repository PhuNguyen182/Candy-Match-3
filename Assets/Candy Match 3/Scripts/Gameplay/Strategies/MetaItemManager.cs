using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Strategies
{
    public class MetaItemManager : IDisposable
    {
        private readonly HashSet<IBlockItem> _blueItems; 
        private readonly HashSet<IBlockItem> _greenItems; 
        private readonly HashSet<IBlockItem> _orangeItems; 
        private readonly HashSet<IBlockItem> _purpleItems; 
        private readonly HashSet<IBlockItem> _redItems; 
        private readonly HashSet<IBlockItem> _yellowItems;
        
        private readonly Dictionary<Vector3Int, IBlockItem> _metaItem;

        public MetaItemManager()
        {
            _metaItem = new();
            _blueItems = new();
            _greenItems = new();
            _orangeItems = new();
            _purpleItems = new();
            _redItems = new();
            _yellowItems = new();
        }

        public IBlockItem Get(Vector3Int position)
        {
            return _metaItem.TryGetValue(position, out IBlockItem item) ? item : null;
        }

        public IBlockItem Add(Vector3Int position, IBlockItem blockItem)
        {
            if (!_metaItem.ContainsKey(position))
                _metaItem.Add(position, blockItem);
            
            else
                _metaItem[position] = blockItem;
            
            return blockItem;
        }

        public void Remove(IBlockItem blockItem)
        {
            if (_metaItem.ContainsKey(blockItem.GridPosition))
                _metaItem.Remove(blockItem.GridPosition);
        }

        public void RemoveAt(Vector3Int position)
        {
            if (_metaItem.ContainsKey(position))
                _metaItem.Remove(position);
        }

        public void ReleaseGridCell(Vector3Int position)
        {
            Add(position, null);
        }

        public void Dispose()
        {
            _metaItem.Clear();
            _blueItems.Clear();
            _greenItems.Clear();
            _orangeItems.Clear();
            _purpleItems.Clear();
            _redItems.Clear();
            _yellowItems.Clear();
        }
    }
}
