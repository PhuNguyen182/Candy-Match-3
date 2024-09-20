using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Misc Object Collection", menuName = "Scriptable Objects/Databases/Misc Object Collection")]
    public class MiscCollection : ScriptableObject
    {
        [PreviewField(60)]
        [SerializeField] public GameObject SpawnerMask;
        [PreviewField(60)]
        [SerializeField] public GameObject CollectibleCheckSign;
    }
}
