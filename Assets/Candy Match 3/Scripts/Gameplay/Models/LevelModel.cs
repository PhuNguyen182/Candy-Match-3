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
        public int TargetMove = 0;
        public ScoreRule ScoreRule = new();

        public List<SpawnRuleBlockData> SpawnerRules = new();
        public List<ColorFillBlockData> BoardFillRule = new();
        public List<ColorFillBlockData> RuledRandomFill = new();
        public List<LevelTargetData> LevelTargetData = new();

        #region Level Board And Items
        public List<BoardBlockPosition> BoardBlockPositions = new();
        public List<BlockItemPosition> ColorBlockItemPositions = new();
        public List<BlockItemPosition> ColorBoosterItemPositions = new();
        public List<BlockItemPosition> BoosterItemPositions = new();
        public List<BlockItemPosition> RandomBlockItemPositions = new();
        public List<BlockItemPosition> RuledRandomBlockPositions = new();
        public List<BlockItemPosition> BreakableItemPositions = new();
        public List<BlockItemPosition> UnbreakableItemPositions = new();
        public List<BlockItemPosition> CollectibleItemPositions = new();
        public List<StatefulBlockPosition> StatefulBlockPositions = new();
        public List<SpawnerBlockPosition> SpawnerBlockPositions = new();
        #endregion

        public void ClearModel()
        {
            TargetMove = 0;
            ScoreRule.Dispose();

            SpawnerRules.Clear();
            BoardFillRule.Clear();
            RuledRandomFill.Clear();
            LevelTargetData.Clear();

            BoardBlockPositions.Clear();
            ColorBlockItemPositions.Clear();
            ColorBoosterItemPositions.Clear();
            BoosterItemPositions.Clear();
            RandomBlockItemPositions.Clear();
            RuledRandomBlockPositions.Clear();
            BreakableItemPositions.Clear();
            UnbreakableItemPositions.Clear();
            CollectibleItemPositions.Clear();
            StatefulBlockPositions.Clear();
            SpawnerBlockPositions.Clear();
        }
    }
}
