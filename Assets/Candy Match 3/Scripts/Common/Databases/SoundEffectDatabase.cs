using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Enums;
using System.Linq;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Sound Effect Database", menuName = "Scriptable Objects/Databases/Sound Effect Database")]
    public class SoundEffectDatabase : ScriptableObject
    {
        [SerializeField] private List<SoundEffectData> soundEffectDatas;

        public Dictionary<SoundEffectType, AudioClip> SoundEffectCollection { get; private set; }

        public void Initialize()
        {
            SoundEffectCollection = soundEffectDatas.ToDictionary(key => key.SoundEffectType, value => value.SoundEffectClip);
        }
    }
}
