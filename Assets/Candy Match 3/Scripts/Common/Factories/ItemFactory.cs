using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.GameItems;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GameItems.Colored;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using GlobalScripts.Factories;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.Common.Factories
{
    public class ItemFactory : BaseFactory<BlockItemData, BaseItem>
    {
        private readonly ItemDatabase _itemDatabase;
        private readonly Transform _itemContainer;

        public ItemFactory(ItemDatabase itemDatabase, Transform itemContainer)
        {
            _itemDatabase = itemDatabase;
            _itemContainer = itemContainer;

            PoolPreload();
        }

        public override BaseItem Produce(BlockItemData param)
        {
            BaseItem blockItem = null;

            blockItem = ProduceColorItem(param);
            if(blockItem != null)
                return blockItem;

            blockItem = ProduceColorBoosterItem(param);
            if (blockItem != null)
                return blockItem;

            blockItem = ProduceBoosterItem(param);
            if (blockItem != null)
                return blockItem;
            
            blockItem = ProduceSpecialItem(param);
            if (blockItem != null)
                return blockItem;

            return blockItem;
        }

        private BaseItem ProduceColorItem(BlockItemData blockItemData)
        {
            for (int i = 0; i < _itemDatabase.ColorModels.Count; i++)
            {
                if(blockItemData.ItemColor == _itemDatabase.ColorModels[i].CandyColor && blockItemData.PrimaryState == 0)
                {
                    ColoredItem coloredItem = SimplePool.Spawn(_itemDatabase.ColoredItem
                                                               , _itemContainer
                                                               , Vector3.zero
                                                               , Quaternion.identity);
                    coloredItem.SetItemID(blockItemData.ID);
                    coloredItem.SetItemType(blockItemData.ItemType);
                    coloredItem.SetColor(blockItemData.ItemColor);
                    coloredItem.ResetItem();
                    return coloredItem;
                }
            }

            return null;
        }

        private BaseItem ProduceColorBoosterItem(BlockItemData blockItemData)
        {
            byte[] boosterProperties = NumericUtils.IntToBytes(blockItemData.PrimaryState);
            ColorBoosterType colorBoosterType = (ColorBoosterType)boosterProperties[1];

            for (int i = 0; i < _itemDatabase.ColorBoosterModels.Count; i++)
            {
                CandyColor candyColor = _itemDatabase.ColorBoosterModels[i].CandyColor;
                ColorBoosterType colorBooster = _itemDatabase.ColorBoosterModels[i].ColorBoosterType;

                if (blockItemData.ItemColor == candyColor && colorBoosterType == colorBooster)
                {
                    ColoredBooster coloredBooster = SimplePool.Spawn(_itemDatabase.ColoredBooster
                                                                     , _itemContainer
                                                                     , Vector3.zero
                                                                     , Quaternion.identity);
                    coloredBooster.SetItemID(blockItemData.ID);
                    coloredBooster.SetItemType(blockItemData.ItemType);
                    coloredBooster.SetColor(blockItemData.ItemColor);
                    coloredBooster.SetBoosterColor(colorBoosterType);
                    coloredBooster.ResetItem();
                    return coloredBooster;
                }
            }

            return null;
        }

        private BaseItem ProduceBoosterItem(BlockItemData blockItemData)
        {
            BaseItem itemPrefab = _itemDatabase.GetBooster(blockItemData.ItemType);

            if(itemPrefab != null)
            {
                BaseItem boosterItem = SimplePool.Spawn(itemPrefab, _itemContainer
                                                       , Vector3.zero, Quaternion.identity);
                boosterItem.SetItemID(blockItemData.ID);
                boosterItem.ResetItem();
                return boosterItem;
            }

            return null;
        }

        private BaseItem ProduceSpecialItem(BlockItemData blockItemData)
        {
            BaseItem itemBlock = null;

            itemBlock = ProduceCollectibleItem(blockItemData);
            if (itemBlock != null)
                return itemBlock;

            itemBlock = ProduceBreakableItem(blockItemData);
            if (itemBlock != null)
                return itemBlock;

            itemBlock = ProduceUnreakableItem(blockItemData);
            if (itemBlock != null)
                return itemBlock;

            return itemBlock;
        }

        private BaseItem ProduceCollectibleItem(BlockItemData blockItemData)
        {
            BaseItem itemPrefab = _itemDatabase.GetItemByType(blockItemData.ItemType);

            if(itemPrefab != null)
            {
                BaseItem collectibleItem = SimplePool.Spawn(itemPrefab, _itemContainer
                                                            , Vector3.zero, Quaternion.identity);
                collectibleItem.SetItemID(blockItemData.ID);
                collectibleItem.ResetItem();
                return collectibleItem;
            }

            return null;
        }

        private BaseItem ProduceBreakableItem(BlockItemData blockItemData)
        {
            BaseItem itemPrefab = _itemDatabase.GetItemByType(blockItemData.ItemType);

            if(itemPrefab != null)
            {
                BaseItem breakableItem = SimplePool.Spawn(itemPrefab, _itemContainer
                                                         , Vector3.zero, Quaternion.identity);
                
                breakableItem.SetItemID(blockItemData.ID);

                if (breakableItem is ISetHealthPoint healthPointSetter)
                    healthPointSetter.SetHealthPoint(blockItemData.HealthPoint);

                breakableItem.ResetItem();
                return breakableItem;
            }

            return null;
        }

        private BaseItem ProduceUnreakableItem(BlockItemData blockItemData)
        {
            BaseItem itemPrefab = _itemDatabase.GetItemByType(blockItemData.ItemType);

            if (itemPrefab != null)
            {
                BaseItem unbreakableItem = SimplePool.Spawn(itemPrefab, _itemContainer
                                                         , Vector3.zero, Quaternion.identity);

                unbreakableItem.SetItemID(blockItemData.ID);
                unbreakableItem.ResetItem();
                return unbreakableItem;
            }

            return null;
        }

        private void PoolPreload()
        {
            SimplePool.PoolPreLoad(_itemDatabase.ColoredItem.gameObject, 30, _itemContainer);
            SimplePool.PoolPreLoad(_itemDatabase.ColoredBooster.gameObject, 15, _itemContainer);
        }
    }
}
