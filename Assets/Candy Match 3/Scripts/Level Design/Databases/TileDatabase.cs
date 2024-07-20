using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.ItemTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.BoardTiles;
using CandyMatch3.Scripts.LevelDesign.CustomTiles.TopTiles;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.LevelDesign.Databases
{
    [CreateAssetMenu(fileName = "Tile Database", menuName = "Scriptable Objects/Level Design/Databases/Tile Database")]
    public class TileDatabase : ScriptableObject
    {
        [SerializeField] private BoardTile boardTile;
        [SerializeField] private List<ColorItemTile> colorItemTiles;
        [SerializeField] private List<BreakableTile> breakableTiles;
        [SerializeField] private List<CollectibleTile> collectibleTiles;
        [SerializeField] private List<TopItemTile> topItemTiles;

        public BoardTile GetBoardTile()
        {
            return boardTile;
        }

        public ColorItemTile GetColorItemTile(int id, ItemType itemType)
        {
            return colorItemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public BreakableTile GetBreakableTile(int id, ItemType itemType, int healthPoint)
        {
            return breakableTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType && tile.HealthPoint == healthPoint);
        }

        public CollectibleTile GetCollectibleTile(int id, ItemType itemType)
        {
            return collectibleTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public TopItemTile GetTopItemTile(int id, TopGroupType groupType, int healthPoint)
        {
            return topItemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.GroupType == groupType && tile.HealthPoint == healthPoint);
        }
    }
}
