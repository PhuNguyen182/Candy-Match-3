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
        [SerializeField] private List<ColorBoosterTile> colorBoosterTiles;
        [Space(3)]
        [SerializeField] private List<SingleBreakableTile> breakableTiles;
        [Space(3)]
        [SerializeField] private List<CollectibleTile> collectibleTiles;
        [Space(3)]
        [SerializeField] private List<BoosterTile> boosterTiles;
        [Space(3)]
        [SerializeField] private List<StatefulTile> statefulTiles;
        [Space(3)]
        [SerializeField] private List<SingleItemTile> itemTiles;

        public BoardTile GetBoardTile()
        {
            return boardTile;
        }

        public ColorItemTile GetColorItemTile(int id, ItemType itemType)
        {
            return colorItemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public ColorBoosterTile GetBoosterTile(int id, CandyColor candyColor, ColorBoosterType boosterType)
        {
            return colorBoosterTiles.FirstOrDefault(tile => tile.ItemID == id && tile.CandyColor == candyColor && tile.ColorBoosterType == boosterType);
        }

        public SingleBreakableTile GetBreakableTile(int id, ItemType itemType, int healthPoint)
        {
            return breakableTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType && tile.HealthPoint == healthPoint);
        }

        public CollectibleTile GetCollectibleTile(int id, ItemType itemType)
        {
            return collectibleTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public StatefulTile GetStatefulTile(int id, StatefulGroupType groupType, int healthPoint)
        {
            return statefulTiles.FirstOrDefault(tile => tile.ItemID == id && tile.GroupType == groupType && tile.HealthPoint == healthPoint);
        }

        public BoosterTile GetBoosterTile(int id, ItemType itemType)
        {
            return boosterTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public SingleItemTile GetItemTile(int id, ItemType itemType)
        {
            return itemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }
    }
}
