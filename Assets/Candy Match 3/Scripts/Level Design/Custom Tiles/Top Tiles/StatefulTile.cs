using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.LevelDesign.LevelBuilders;

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
            int boardFlag = gridInformation.GetPositionProperty(position, BoardConstants.BoardTileValidate, 0);
            return boardFlag == 1;
        }
    }
}
