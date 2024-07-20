using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Collectible Tile", menuName = "Scriptable Objects/Level Design/Tiles/Collectible Tile")]
    public class CollectibleTile : BaseMapTile
    {
        [SerializeField] private ItemType itemType;

        public ItemType ItemType => itemType;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
