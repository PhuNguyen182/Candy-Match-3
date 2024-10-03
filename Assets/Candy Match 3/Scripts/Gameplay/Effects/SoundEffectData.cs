using System;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    [Serializable]
    public struct SoundEffectData
    {
        public SoundEffectType SoundEffectType;
        public AudioClip SoundEffectClip;
    }

    [Serializable]
    public struct MusicData
    {
        public BackgroundMusicType BackgroundMusicType;
        public AudioClip BackgroundMusicClip;
    }
}
