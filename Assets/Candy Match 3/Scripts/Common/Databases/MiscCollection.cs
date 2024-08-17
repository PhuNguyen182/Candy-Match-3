using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Misc Object Collection", menuName = "Scriptable Objects/Databases/Misc Object Collection")]
    public class MiscCollection : ScriptableObject
    {
        [SerializeField] public GameObject SpawnerMask;
        [SerializeField] public GameObject CollectibleCheckSign;
    }
}
