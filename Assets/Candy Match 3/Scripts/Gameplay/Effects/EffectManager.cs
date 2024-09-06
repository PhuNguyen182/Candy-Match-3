using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.Miscs;
using GlobalScripts.Effects;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private SoundEffectDatabase soundEffectDatabase;
        [SerializeField] private TargetCompletedObject targetObject;

        public static EffectManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            PreloadEffects();
            soundEffectDatabase.Initialize();
        }

        public void PlaySoundEffect(SoundEffectType soundEffect)
        {
            AudioClip sound = soundEffectDatabase.SoundEffectCollection[soundEffect];
            ItemSoundEffect itemSoundEffect = SimplePool.Spawn(effectDatabase.SoundEffect
                                                               , EffectContainer.Transform
                                                               , Vector3.zero, Quaternion.identity);
            itemSoundEffect.PlaySound(sound);
        }

        public void SpawnFlyCompletedTarget(Vector3 position)
        {
            SimplePool.Spawn(targetObject, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void SpawnNewCreatedEffect(Vector3 position)
        {
            SimplePool.Spawn(effectDatabase.SpawnBoosterEffect, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void SpawnColorEffect(CandyColor candyColor, Vector3 position)
        {
            GameObject effect = candyColor switch
            {
                CandyColor.Blue => effectDatabase.BlueMatchEffect,
                CandyColor.Green => effectDatabase.GreenMatchEffect,
                CandyColor.Orange => effectDatabase.OrangeMatchEffect,
                CandyColor.Purple => effectDatabase.PurpleMatchEffect,
                CandyColor.Red => effectDatabase.RedMatchEffect,
                CandyColor.Yellow => effectDatabase.YellowMatchEffect,
                _ => null
            };

            if (effect != null)
                SimplePool.Spawn(effect, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void SpawnSpecialEffect(ItemType itemType, Vector3 position)
        {
            GameObject effect = itemType switch
            {
                ItemType.Biscuit => effectDatabase.BiscuitBreakEffect,
                ItemType.Chocolate => effectDatabase.ChocolateBreakEffect,
                ItemType.Marshmallow => effectDatabase.MarshmallowBreakEffect,
                ItemType.Cherry => effectDatabase.ChocolateBreakEffect,
                ItemType.Watermelon => effectDatabase.ChocolateBreakEffect,
                _ => null
            };

            if (effect != null)
                SimplePool.Spawn(effect, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void SpawnBoosterEffect(ItemType itemType, BoosterType boosterType, Vector3 position)
        {
            GameObject effect = null;

            if (itemType == ItemType.ColorBomb)
                effect = effectDatabase.ColorfulEffect;

            else
            {
                effect = boosterType switch
                {
                    BoosterType.Horizontal => effectDatabase.StripedHorizontal,
                    BoosterType.Vertical => effectDatabase.StripedVertical,
                    BoosterType.Wrapped => effectDatabase.WrappedEffect,
                    _ => null
                };
            }

            if (effect != null)
                SimplePool.Spawn(effect, EffectContainer.Transform, position, effect.transform.rotation);
        }

        public void SpawnStatefulEffect(StatefulGroupType statefulType, Vector3 position)
        {
            GameObject effect = statefulType switch
            {
                StatefulGroupType.Ice => effectDatabase.IceEffect,
                StatefulGroupType.Honey => effectDatabase.HoneyEffect,
                StatefulGroupType.Syrup => effectDatabase.SyrupEffect,
                _ => null
            };

            if (effect != null)
                SimplePool.Spawn(effect, EffectContainer.Transform, position, Quaternion.identity);
        }

        private void PreloadEffects()
        {
            SimplePool.PoolPreLoad(effectDatabase.SoundEffect.gameObject, 20, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.BlueMatchEffect, 15, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.GreenMatchEffect, 15, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.OrangeMatchEffect, 15, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.PurpleMatchEffect, 15, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.RedMatchEffect, 15, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.YellowMatchEffect, 15, EffectContainer.Transform);

            SimplePool.PoolPreLoad(effectDatabase.SpawnBoosterEffect, 10, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.ColorfulEffect, 10, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.StripedHorizontal, 10, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.StripedVertical, 10, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.WrappedEffect, 10, EffectContainer.Transform);

            SimplePool.PoolPreLoad(effectDatabase.ColorfulFireray.gameObject, 30, EffectContainer.Transform);
        }
    }
}
