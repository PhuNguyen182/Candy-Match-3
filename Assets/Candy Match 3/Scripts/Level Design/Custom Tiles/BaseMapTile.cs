using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    public class BaseMapTile : Tile
    {
        [SerializeField] protected int itemId;

        public int ItemID => itemId;

        public virtual bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return true;
        }
    }
}
