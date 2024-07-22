using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    public class BaseItemTile : BaseMapTile
    {
        [SerializeField] protected ItemType itemType;

        [Header("Tile States")]
        [SerializeField] protected int primaryState;
        [SerializeField] protected int secondaryState;

        public ItemType ItemType => itemType;
    }
}
