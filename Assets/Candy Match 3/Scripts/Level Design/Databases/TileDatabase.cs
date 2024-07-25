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
        [SerializeField] private RandomTile randomTile;
        [Space(3)]
        [SerializeField] private UnbreakableTile unbreakableTile;
        [Space(3)]
        [SerializeField] private List<RuledRandomTile> ruledRandomTiles;
        [Space(3)]
        [SerializeField] private List<SpawnerTile> spawnerTiles;
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

        public BoardTile GetBoardTile()
        {
            return boardTile;
        }

        public RandomTile GetRandomTile()
        {
            return randomTile;
        }

        public UnbreakableTile GetUnbreakableTile()
        {
            return unbreakableTile;
        }

        public RuledRandomTile GetRuledRandomTile(int id)
        {
            return ruledRandomTiles.FirstOrDefault(tile => tile.ItemID == id);
        }

        public SpawnerTile GetSpawnerTile(int id)
        {
            return spawnerTiles.FirstOrDefault(tile => tile.ItemID == id);
        }

        public ColorItemTile GetColorItemTile(int id, ItemType itemType)
        {
            return colorItemTiles.FirstOrDefault(tile => tile.ItemID == id && tile.ItemType == itemType);
        }

        public ColorBoosterTile GetColorBoosterTile(int id, CandyColor candyColor, ColorBoosterType boosterType)
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
    }
}
