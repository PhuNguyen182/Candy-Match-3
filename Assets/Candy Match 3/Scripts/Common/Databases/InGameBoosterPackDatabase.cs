using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "In-Game Booster Pack Database", menuName = "Scriptable Objects/Databases/In-Game Booster Pack Database")]
    public class InGameBoosterPackDatabase : ScriptableObject
    {
        [SerializeField] private List<InGameBoosterPack> inGameBoosterPacks;

        private Dictionary<InGameBoosterType, InGameBoosterPack> _boosterPackCollections;

        public Dictionary<InGameBoosterType, InGameBoosterPack> BoosterPackCollections
        {
            get
            {
                if(_boosterPackCollections == null)
                {
                    _boosterPackCollections = inGameBoosterPacks.ToDictionary(key => key.BoosterType, value => value);
                }

                return _boosterPackCollections;
            }
        }
    }
}
