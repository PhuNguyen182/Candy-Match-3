using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Item Tile")]
    public class ItemTile : BaseMapTile
    {
        [SerializeField] private int itemId;
        [SerializeField] private string itemName;
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemColor itemColor;

        public int ItemID => itemId;
        public ItemType ItemType => itemType;
        public ItemColor ItemColor => itemColor;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
