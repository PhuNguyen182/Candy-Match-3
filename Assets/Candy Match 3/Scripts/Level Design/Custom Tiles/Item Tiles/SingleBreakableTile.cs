using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles
{
    [CreateAssetMenu(fileName = "Breakable Tile", menuName = "Scriptable Objects/Level Design/Tiles/Breakable Tile")]
    public class SingleBreakableTile : SingleItemTile
    {
        [Space(10)]
        [SerializeField] private int healthPoint = 1;

        public int HealthPoint => healthPoint;
    }
}
