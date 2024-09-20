using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Stateful Sprite Database", menuName = "Scriptable Objects/Databases/Stateful Sprite Database")]
    public class StatefulSpriteDatabase : ScriptableObject
    {
        [PreviewField(60)]
        [SerializeField] public Sprite IceState;
        [PreviewField(60)]
        [SerializeField] public Sprite HoneyState;
        [PreviewField(60)]
        [SerializeField] public Sprite[] SyrupStates;
    }
}
