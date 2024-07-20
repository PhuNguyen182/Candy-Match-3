using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.LevelDesign.CustomTiles;
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
        [Space(3)]
        [SerializeField] private List<ColorItemTile> colorItemTiles;
        [Space(3)]
        [SerializeField] private List<BreakableTile> breakableTiles;
        [Space(3)]
        [SerializeField] private List<CollectibleTile> collectibleTiles;
        [Space(3)]
        [SerializeField] private List<BoosterTile> boosterTiles;
        [Space(3)]
        [SerializeField] private List<TopItemTile> topItemTiles;
        [Space(3)]
        [SerializeField] private List<ItemTile> itemTiles;

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

        public BoosterTile GetBoosterTile(int id, ItemType itemType)
        {
            return boosterTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public ItemTile GetItemTile(int id, ItemType itemType)
        {
            return itemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }
    }
}
