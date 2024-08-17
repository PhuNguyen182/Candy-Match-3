using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Effects;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Effect Database", menuName = "Scriptable Objects/Databases/Effect Database")]
    public class EffectDatabase : ScriptableObject
    {
        [SerializeField] public ItemSoundEffect SoundEffect;
    }
}
