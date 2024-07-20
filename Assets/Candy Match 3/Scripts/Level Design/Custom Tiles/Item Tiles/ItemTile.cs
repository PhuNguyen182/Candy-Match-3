using CandyMatch3.Scripts.Common.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Item Tile")]
    public class ItemTile : BaseMapTile
    {
        [SerializeField] protected ItemType itemType;

        public ItemType ItemType => itemType;
    }
}
