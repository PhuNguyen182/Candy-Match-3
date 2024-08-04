using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models;
using CandyMatch3.Scripts.Gameplay.GameItems;
using CandyMatch3.Scripts.Gameplay.GameItems.Colored;
using CandyMatch3.Scripts.Gameplay.GameItems.Boosters;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Scriptable Objects/Databases/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Color Models")]
        [SerializeField] public List<ItemColorModel> ColorModels;
        [SerializeField] public List<ItemColorModel> ColorBoosterModels;

        [Header("Items")]
        [SerializeField] public ColoredItem ColoredItem;
        [SerializeField] public ColoredBooster ColoredBooster;
        [SerializeField] public List<BaseBoosterItem> Boosters;
        [SerializeField] public List<BaseItem> SpecialItems;

        public BaseBoosterItem GetBooster(ItemType boosterType)
        {
            return Boosters.FirstOrDefault(booster => booster.ItemType == boosterType);
        }

        public BaseItem GetItemByType(ItemType itemType)
        {
            return SpecialItems.FirstOrDefault(item => item.ItemType == itemType);
        }
    }
}
