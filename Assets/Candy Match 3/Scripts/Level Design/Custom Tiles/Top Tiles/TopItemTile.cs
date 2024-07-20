using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.TopTiles
{
    [CreateAssetMenu(fileName = "Top Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Top Item Tile")]
    public class TopItemTile : BaseMapTile
    {
        [SerializeField] private int healthPoint;
        [SerializeField] private TopGroupType groupType;

        public int HealthPoint => healthPoint;
        public TopGroupType GroupType => groupType;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
