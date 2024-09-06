using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.Miscs;
using Cysharp.Threading.Tasks;
using GlobalScripts.Effects;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private SoundEffectDatabase soundEffectDatabase;
        [SerializeField] private TargetCompletedObject targetObject;

        private CancellationToken _token;
        public static EffectManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void Start()
        {
            PreloadEffects().Forget();
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

        private async UniTask PreloadEffects()
        {
            SimplePool.PoolPreLoad(effectDatabase.SoundEffect.gameObject, 20, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.BlueMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.GreenMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.OrangeMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.PurpleMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.RedMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.YellowMatchEffect, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.SpawnBoosterEffect, 6, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.ColorfulEffect, 6, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.StripedHorizontal, 6, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.StripedVertical, 6, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.WrappedEffect, 6, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.ColorfulFireray.gameObject, 20, EffectContainer.Transform);
        }
    }
}
