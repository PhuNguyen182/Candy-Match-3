using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Music Database", menuName = "Scriptable Objects/Databases/Music Database")]
    public class MusicDatabase : ScriptableObject
    {
        [SerializeField] private List<MusicData> backgroundMusics;

        public Dictionary<BackgroundMusicType, AudioClip> BackgroundMusicCollection { get; private set; }

        public void Initialize()
        {
            BackgroundMusicCollection ??= backgroundMusics.ToDictionary(key => key.BackgroundMusicType, value => value.BackgroundMusicClip);
        }

        public AudioClip GetBackgroundMusic(BackgroundMusicType backgroundMusicType)
        {
            return BackgroundMusicCollection.TryGetValue(backgroundMusicType, out var clip) ? clip : null;
        }
    }
}
