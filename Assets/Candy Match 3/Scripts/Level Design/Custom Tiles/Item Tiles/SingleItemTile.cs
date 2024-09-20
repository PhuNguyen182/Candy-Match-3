using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.LevelDesign.LevelBuilders;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Single Item Tile")]
    public class SingleItemTile : BaseItemTile
    {
        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            int boardFlag = gridInformation.GetPositionProperty(position, BoardConstants.BoardTileValidate, 0);
            return boardFlag == 1;
        }
    }
}
