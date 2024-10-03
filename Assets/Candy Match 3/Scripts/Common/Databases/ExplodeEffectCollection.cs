using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.DataStructs;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Explode Effect Collection", menuName = "Scriptable Objects/Databases/Explode Effect Collection")]
    public class ExplodeEffectCollection : ScriptableObject
    {
        [SerializeField] public ExplodeEffectData SingleWrappedExplode;
        [SerializeField] public ExplodeEffectData DoubleWrappedExplode;
    }
}
