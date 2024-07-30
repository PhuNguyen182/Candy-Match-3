using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Factories;

namespace CandyMatch3.Scripts.Gameplay.Strategies
{
    public class ItemManager
    {
        private readonly GridCellManager _gridCellManager;
        private readonly MetaItemManager _metaItemManager;
        private readonly ItemFactory _itemFactory;

        public ItemManager(GridCellManager gridCellManager, MetaItemManager metaItemManager, ItemFactory itemFactory)
        {
            _gridCellManager = gridCellManager;
            _metaItemManager = metaItemManager;
            _itemFactory = itemFactory;
        }

        public IBlockItem Create(BlockItemData itemData)
        {
            IBlockItem item = _itemFactory.Produce(itemData);
            return item;
        }

        public IBlockItem Add(BlockItemData itemData)
        {
            IBlockItem item = Create(itemData);
            return _metaItemManager.Add(item);
        }

        public void Remove(IBlockItem item)
        {
            if (item != null)
            {
                _metaItemManager.Remove(item);
                IGridCell gridCell = _gridCellManager.Get(item.GridPosition);
                gridCell.SetBlockItem(null);
            }
        }
    }
}
