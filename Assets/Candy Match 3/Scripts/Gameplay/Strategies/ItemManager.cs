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

        public IBlockItem Get(Vector3Int position)
        {
            return _metaItemManager.Get(position);
        }

        public IBlockItem Create(BlockItemPosition itemData)
        {
            IBlockItem item = _itemFactory.Produce(itemData.ItemData);
            return _metaItemManager.Add(itemData.Position, item);
        }

        public IBlockItem Add(BlockItemPosition itemData)
        {
            IBlockItem item = Create(itemData);
            IGridCell gridCell = _gridCellManager.Get(itemData.Position);

            gridCell.SetBlockItem(item);
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

        public ItemType GetItemTypeFromColorAndBoosterType(CandyColor color, BoosterType boosterType)
        {
            ItemType itemType = ItemType.None;

            switch (boosterType)
            {
                case BoosterType.Horizontal:
                    itemType = GetHorizontal(color);
                    break;
                case BoosterType.Vertical:
                    itemType = GetVertical(color);
                    break;
                case BoosterType.Wrapped:
                    itemType = GetWrapped(color);
                    break;
            }

            return itemType;
        }

        public (ItemType, BoosterType) GetBoosterTypeFromMatch(MatchType matchType, CandyColor candyColor)
        {
            if (matchType == MatchType.Match5)
                return (ItemType.ColorBomb, BoosterType.None);

            BoosterType boosterColor = matchType switch
            {
                MatchType.Match4Horizontal => BoosterType.Vertical,
                MatchType.Match4Vertical => BoosterType.Horizontal,
                MatchType.MatchL => BoosterType.Wrapped,
                MatchType.MatchT => BoosterType.Wrapped,
                _ => BoosterType.None
            };

            ItemType booster = _itemFactory.FindColorBooster(boosterColor, candyColor);
            return (booster, boosterColor);
        }

        private ItemType GetWrapped(CandyColor candyColor)
        {
            ItemType itemType = candyColor switch
            {
                CandyColor.Blue => ItemType.BlueWrapped,
                CandyColor.Green => ItemType.GreenWrapped,
                CandyColor.Orange => ItemType.OrangeWrapped,
                CandyColor.Purple => ItemType.PurpleWrapped,
                CandyColor.Red => ItemType.RedWrapped,
                CandyColor.Yellow => ItemType.YellowWrapped,
                _ => ItemType.None
            };

            return itemType;
        }

        private ItemType GetHorizontal(CandyColor candyColor)
        {
            ItemType itemType = candyColor switch
            {
                CandyColor.Blue => ItemType.BlueHorizontal,
                CandyColor.Green => ItemType.GreenHorizontal,
                CandyColor.Orange => ItemType.OrangeHorizontal,
                CandyColor.Purple => ItemType.PurpleHorizontal,
                CandyColor.Red => ItemType.RedHorizontal,
                CandyColor.Yellow => ItemType.YellowHorizontal,
                _ => ItemType.None
            };

            return itemType;
        }

        private ItemType GetVertical(CandyColor candyColor)
        {
            ItemType itemType = candyColor switch
            {
                CandyColor.Blue => ItemType.BlueVertical,
                CandyColor.Green => ItemType.GreenVertical,
                CandyColor.Orange => ItemType.OrangeVertical,
                CandyColor.Purple => ItemType.PurpleVertical,
                CandyColor.Red => ItemType.RedVertical,
                CandyColor.Yellow => ItemType.YellowVertical,
                _ => ItemType.None
            };

            return itemType;
        }
    }
}
