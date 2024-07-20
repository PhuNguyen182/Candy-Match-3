using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Breakable Tile", menuName = "Scriptable Objects/Level Design/Tiles/Breakable Tile")]
    public class BreakableTile : BaseMapTile
    {
        [SerializeField] private int healthPoint = 1;
        [SerializeField] private ItemType itemType;

        public int HealthPoint => healthPoint;
        public ItemType ItemType => itemType;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
