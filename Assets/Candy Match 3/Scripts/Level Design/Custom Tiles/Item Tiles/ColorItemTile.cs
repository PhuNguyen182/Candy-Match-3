using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Color Item Tile", menuName = "Scriptable Objects/Level Design/Tiles/Color Item Tile")]
    public class ColorItemTile : SingleItemTile
    {
        [SerializeField] private CandyColor candyColor;

        public CandyColor CandyColor => candyColor;
    }
}
