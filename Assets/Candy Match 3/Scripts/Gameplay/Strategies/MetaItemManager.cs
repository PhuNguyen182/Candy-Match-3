using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Strategies
{
    public class MetaItemManager : IDisposable
    {
        private readonly Dictionary<Vector3Int, IBlockItem> _metaItem = new();

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

        public void Dispose()
        {
            _metaItem.Clear();
        }
    }
}
