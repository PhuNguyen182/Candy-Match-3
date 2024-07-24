using System;
using System.Collections.Generic;

namespace CandyMatch3.Scripts.Gameplay.Models
{
    [Serializable]
    public class SpawnRule
    {
        public int ID;
        public List<ColorFillData> ColorFillDatas = new();
    }
}
