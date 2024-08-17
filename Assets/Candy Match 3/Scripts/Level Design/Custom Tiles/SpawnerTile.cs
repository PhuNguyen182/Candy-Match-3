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
        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            int boardProperty = gridInformation.GetPositionProperty(position, BoardConstants.BoardTileValidate, 0);
            int upBoardProperty = gridInformation.GetPositionProperty(position + Vector3Int.up, BoardConstants.BoardTileValidate, 0);
            int spawnerProperty = gridInformation.GetPositionProperty(position + Vector3Int.up, BoardConstants.SpawnerTileValidate, 0);
            
            return boardProperty == 1 && upBoardProperty != 1 && spawnerProperty == 1;
        }
    }
}
