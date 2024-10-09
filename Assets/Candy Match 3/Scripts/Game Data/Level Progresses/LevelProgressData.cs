using System;
using System.Linq;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.CustomData;

namespace CandyMatch3.Scripts.GameData.LevelProgresses
{
    [Serializable]
    public class LevelProgressData
    {
        public int Level = 1;
        public List<LevelProgressNodeData> LevelProgresses;

        public void SetLevel(int level)
        {
            Level = level;
        }

        public LevelProgressData()
        {
            LevelProgresses = new();
        }

        public LevelProgressNodeData GetLevelProgress(int level)
        {
            return LevelProgresses.FirstOrDefault(x => x.DataValue.Level == level);
        }

        public bool IsLevelComplete(int level)
        {
            return LevelProgresses.Exists(x => x.DataValue.Level == level);
        }

        public void Append(LevelProgressNode progress)
        {
            if (LevelProgresses.Any(p => p.DataValue.Level == progress.Level))
            {
                LevelProgressNodeData levelProgress = GetLevelProgress(progress.Level);

                if (progress.Stars >= levelProgress.DataValue.Stars)
                {
                    int index = LevelProgresses.FindIndex(p => p.DataValue.Level == progress.Level);
                    LevelProgresses[index] = levelProgress;
                }
            }

            else
            {
                LevelProgresses.Add(new LevelProgressNodeData
                {
                    DataValue = progress
                });
            }
        }
    }
}