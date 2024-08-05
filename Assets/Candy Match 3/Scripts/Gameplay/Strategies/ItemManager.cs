using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Common.Factories;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;

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

        public IBlockItem Create(BlockItemPosition itemData)
        {
            IBlockItem item = _itemFactory.Produce(itemData.ItemData);
            return item;
        }

        public IBlockItem Add(BlockItemPosition itemData)
        {
            IBlockItem item = Create(itemData);
            IGridCell gridCell = _gridCellManager.Get(itemData.Position);

            gridCell.SetBlockItem(item);
            item.SetWorldPosition(gridCell.WorldPosition);

            return _metaItemManager.Add(itemData.Position, item);
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

        public ItemType GetItemTypeFromColor(CandyColor color)
        {
            ItemType itemType = color switch
            {
                CandyColor.Blue => ItemType.Blue,
                CandyColor.Green => ItemType.Green,
                CandyColor.Orange => ItemType.Orange,
                CandyColor.Purple => ItemType.Purple,
                CandyColor.Red => ItemType.Red,
                CandyColor.Yellow => ItemType.Yellow,
                _ => ItemType.None
            };

            return itemType;
        }
    }
}
