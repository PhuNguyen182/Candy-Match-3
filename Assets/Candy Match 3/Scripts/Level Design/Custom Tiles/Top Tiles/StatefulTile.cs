using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.TopTiles
{
    [CreateAssetMenu(fileName = "Stateful Tile", menuName = "Scriptable Objects/Level Design/Tiles/Stateful Tile")]
    public class StatefulTile : BaseMapTile
    {
        [SerializeField] private int healthPoint;
        [SerializeField] private StatefulGroupType groupType;

        public int HealthPoint => healthPoint;
        public StatefulGroupType GroupType => groupType;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
