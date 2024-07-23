using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomData;

namespace CandyMatch3.Scripts.Gameplay.Models
{
    [Serializable]
    public class LevelModel
    {
        public List<BoardBlockPosition> BoardBlockPositions = new();
        public List<BlockItemPosition> ColorBlockItemPositions = new();
        public List<BlockItemPosition> ColorBoosterItemPositions = new();
        public List<BlockItemPosition> BoosterItemPositions = new();
        public List<BlockItemPosition> RandomBlockItemPositions = new();
        public List<BlockItemPosition> BreakableItemPositions = new();
        public List<BlockItemPosition> UnbreakableItemPositions = new();
        public List<BlockItemPosition> CollectibleItemPositions = new();
        public List<StatefulBlockPosition> StatefulBlockPositions = new();
        public List<SpawnerBlockPosition> SpawnerBlockPositions = new();

        public void ClearModel()
        {
            BoardBlockPositions.Clear();
            ColorBlockItemPositions.Clear();
            ColorBoosterItemPositions.Clear();
            BoosterItemPositions.Clear();
            RandomBlockItemPositions.Clear();
            BreakableItemPositions.Clear();
            UnbreakableItemPositions.Clear();
            CollectibleItemPositions.Clear();
            StatefulBlockPositions.Clear();
            SpawnerBlockPositions.Clear();
        }
    }
}
