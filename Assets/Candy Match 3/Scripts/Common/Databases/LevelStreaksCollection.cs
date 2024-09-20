using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Level Streaks Collection", menuName = "Scriptable Objects/Databases/Level Streaks Collection")]
    public class LevelStreaksCollection : ScriptableObject
    {
        [SerializeField] public Range<int>[] LevelStreaks;
    }
}
