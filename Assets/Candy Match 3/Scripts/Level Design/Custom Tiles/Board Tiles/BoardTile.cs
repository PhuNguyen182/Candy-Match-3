using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles
{
    [CreateAssetMenu(fileName = "Board Tile", menuName = "Scriptable Objects/Level Design/Tiles/Board Tile")]
    public class BoardTile : BaseMapTile
    {
        [SerializeField] private Sprite oddSprite;
        [SerializeField] private Sprite evenSprite;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            
            if ((position.x % 2 == 0 && position.y % 2 == 0) || (position.x % 2 != 0 && position.y % 2 != 0))
                sprite = evenSprite;
            
            else if((position.x % 2 == 0 && position.y % 2 != 0) || (position.x % 2 != 0 && position.y % 2 == 0))
                sprite = oddSprite;
        }
    }
}
