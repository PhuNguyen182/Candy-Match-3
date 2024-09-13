using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Target Database", menuName = "Scriptable Objects/Databases/Target Database")]
    public class TargetDatabase : ScriptableObject
    {
        [SerializeField] public List<TargetDisplayData> TargetDatas;

        private Dictionary<TargetEnum, Sprite> _targetCollection;

        public void Initialize()
        {
            _targetCollection = TargetDatas.ToDictionary(key => key.Target, value => value.Icon);
        }

        public Sprite GetTargetIcon(TargetEnum targetEnum)
        {
            return _targetCollection.TryGetValue(targetEnum, out Sprite sprite) ? sprite : null;
        }
    }
}
