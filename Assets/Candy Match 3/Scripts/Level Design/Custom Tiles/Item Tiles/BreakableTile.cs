using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Breakable Tile", menuName = "Scriptable Objects/Level Design/Tiles/Breakable Tile")]
    public class BreakableTile : ItemTile
    {
        [SerializeField] private int healthPoint = 1;

        public int HealthPoint => healthPoint;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
