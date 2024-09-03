using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Color Booster Tile", menuName = "Scriptable Objects/Level Design/Tiles/Color Booster Tile")]
    public class ColorBoosterTile : SingleItemTile
    {
        [SerializeField] private CandyColor candyColor;
        [SerializeField] private BoosterType colorBoosterType;

        public CandyColor CandyColor => candyColor;
        public BoosterType ColorBoosterType => colorBoosterType;
    }
}
