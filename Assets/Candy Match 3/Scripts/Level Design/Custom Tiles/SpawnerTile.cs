using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Spawner Tile", menuName = "Scriptable Objects/Level Design/Tiles/Spawner Tile")]
    public class SpawnerTile : BaseMapTile
    {
        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
