using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Stateful Sprite Database", menuName = "Scriptable Objects/Databases/Stateful Sprite Database")]
    public class StatefulSpriteDatabase : ScriptableObject
    {
        [SerializeField] public Sprite IceState;
        [SerializeField] public Sprite HoneyState;
        [SerializeField] public Sprite[] SyrupStates;
    }
}
