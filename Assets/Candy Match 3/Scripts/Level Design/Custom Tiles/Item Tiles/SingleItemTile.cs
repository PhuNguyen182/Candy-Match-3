using CandyMatch3.Scripts.Common.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Single Item Tile")]
    public class SingleItemTile : BaseItemTile
    {
        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
