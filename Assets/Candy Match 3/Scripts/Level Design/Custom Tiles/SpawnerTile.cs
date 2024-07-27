using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.LevelDesign.LevelBuilder;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Spawner Tile", menuName = "Scriptable Objects/Level Design/Tiles/Spawner Tile")]
    public class SpawnerTile : BaseMapTile
    {
        private Vector3Int _offset = new(0, -1, 0);

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            int boardProperty = gridInformation.GetPositionProperty(position, BoardConstants.BoardTileValidate, 0);
            
            if (boardProperty == 1)
                return false;

            boardProperty = gridInformation.GetPositionProperty(position + _offset, BoardConstants.BoardTileValidate, 0);
            if (boardProperty != 1)
                return false;

            return true;
        }
    }
}
