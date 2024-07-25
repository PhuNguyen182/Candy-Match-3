using System.Collections;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.Enums;
using UnityEngine;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles
{
    [CreateAssetMenu(fileName = "Ruled Random Tile", menuName = "Scriptable Objects/Level Design/Tiles/Ruled Random Tile")]
    public class RuledRandomTile : SingleItemTile
    {
        [SerializeField] private CandyColor candyColor;

        public CandyColor CandyColor => candyColor;
    }
}
