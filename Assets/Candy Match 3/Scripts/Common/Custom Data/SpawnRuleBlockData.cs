using System;
using System.Collections.Generic;

namespace CandyMatch3.Scripts.Common.CustomData
{
    [Serializable]
    public class SpawnRuleBlockData
    {
        public int ID;
        public List<ItemSpawnerData> ItemSpawnerData = new();
    }
}
