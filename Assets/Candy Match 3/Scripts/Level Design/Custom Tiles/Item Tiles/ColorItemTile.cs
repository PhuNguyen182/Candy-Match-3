using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Color Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Color Item Tile")]
    public class ColorItemTile : BaseMapTile
    {
        [SerializeField] private ItemType itemType;
        [SerializeField] private CandyColor candyColor;
        [SerializeField] private ColorType colorType;

        public ItemType ItemType => itemType;
        public CandyColor CandyColor => candyColor;
        public ColorType ColorType => colorType;

        public override bool ValidateTile(Vector3Int position, Tilemap tilemap, GridInformation gridInformation)
        {
            return base.ValidateTile(position, tilemap, gridInformation);
        }
    }
}
