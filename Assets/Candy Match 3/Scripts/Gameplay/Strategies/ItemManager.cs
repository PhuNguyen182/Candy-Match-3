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

        public CandyColor GetColorFromItemType(ItemType itemType)
        {
            CandyColor candyColor = itemType switch
            {
                ItemType.Blue => CandyColor.Blue,
                ItemType.BlueHorizontal => CandyColor.Blue,
                ItemType.BlueVertical => CandyColor.Blue,
                ItemType.BlueWrapped => CandyColor.Blue,

                ItemType.Green => CandyColor.Green,
                ItemType.GreenHorizontal => CandyColor.Green,
                ItemType.GreenVertical => CandyColor.Green,
                ItemType.GreenWrapped => CandyColor.Green,

                ItemType.Orange => CandyColor.Orange,
                ItemType.OrangeHorizontal => CandyColor.Orange,
                ItemType.OrangeVertical => CandyColor.Orange,
                ItemType.OrangeWrapped => CandyColor.Orange,

                ItemType.Purple => CandyColor.Purple,
                ItemType.PurpleHorizontal => CandyColor.Purple,
                ItemType.PurpleVertical => CandyColor.Purple,
                ItemType.PurpleWrapped => CandyColor.Purple,

                ItemType.Red => CandyColor.Red,
                ItemType.RedHorizontal => CandyColor.Red,
                ItemType.RedVertical => CandyColor.Red,
                ItemType.RedWrapped => CandyColor.Red,

                ItemType.Yellow => CandyColor.Yellow,
                ItemType.YellowHorizontal => CandyColor.Yellow,
                ItemType.YellowVertical => CandyColor.Yellow,
                ItemType.YellowWrapped => CandyColor.Yellow,
                _ => CandyColor.None
            };

            return candyColor;
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

        public (ItemType, ColorBoosterType) GetBoosterTypeFromMatch(MatchType matchType, CandyColor candyColor)
        {
            if (matchType == MatchType.Match5)
                return (ItemType.ColorBomb, ColorBoosterType.None);

            ColorBoosterType boosterColor = matchType switch
            {
                MatchType.Match4Horizontal => ColorBoosterType.Horizontal,
                MatchType.Match4Vertical => ColorBoosterType.Vertical,
                MatchType.MatchL => ColorBoosterType.Wrapped,
                MatchType.MatchT => ColorBoosterType.Wrapped,
                _ => ColorBoosterType.None
            };

            ItemType booster = _itemFactory.FindColorBooster(boosterColor, candyColor);
            return (booster, boosterColor);
        }
    }
}
