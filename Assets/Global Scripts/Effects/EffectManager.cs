using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Databases;

namespace GlobalScripts.Effects
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private EffectDatabase effectDatabase;

        public static EffectManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void PreloadEffects()
        {
            SimplePool.PoolPreLoad(effectDatabase.SoundEffect.gameObject, 20, EffectContainer.Transform);
        }

        public void PlaySound(AudioClip audioClip)
        {
            ItemSoundEffect soundEffect = SimplePool.Spawn(effectDatabase.SoundEffect, EffectContainer.Transform, Vector3.zero, Quaternion.identity);
            soundEffect.PlaySound(audioClip);
        }
    }
}
