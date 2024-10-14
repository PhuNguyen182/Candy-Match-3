using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Effects;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.Miscs;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Effects
{
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private EffectDatabase effectDatabase;
        [SerializeField] private TargetDatabase targetDatabase;
        [SerializeField] private SoundEffectDatabase soundEffectDatabase;
        [SerializeField] private TargetCompletedObject targetObject;
        [SerializeField] private float playSoundRate = 0.1f;

        private float _playSoundNext = 0;
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

        public void PlaySoundEffect(SoundEffectType soundEffect, bool useGapTime = false)
        {
            if (useGapTime)
            {
                if (_playSoundNext < Time.fixedTime)
                {
                    _playSoundNext = Time.fixedTime + playSoundRate;
                    PlaySoundEffect(soundEffect);
                }
            }

            else
                PlaySoundEffect(soundEffect);
        }

        public void PlayItemSwapEffect(Vector3 position)
        {
            SimplePool.Spawn(effectDatabase.ItemSwapEffect, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void PlayShuffleEffect()
        {
            SimplePool.Spawn(effectDatabase.ShuffleEffect, EffectContainer.Transform, Vector3.zero, Quaternion.identity);
        }

        public void ShowCompliment(ComplimentEnum compliment)
        {
            ComplimentText complimentText = SimplePool.Spawn(effectDatabase.Compliment, UIEffectContainer.Transform
                                                            , UIEffectContainer.Transform.position, Quaternion.identity);
            complimentText.transform.localScale = Vector3.one;
            complimentText.ShowCompliment(compliment);
        }

        public TargetCompletedObject SpawnFlyCompletedTarget(TargetEnum targetType, Vector3 position)
        {
            Sprite target = targetDatabase.GetTargetIcon(targetType);
            TargetCompletedObject flyObject = SimplePool.Spawn(targetObject, EffectContainer.Transform
                                                               , position, Quaternion.identity);
            flyObject.SetItemIcon(target);
            return flyObject;
        }

        public void SpawnNewCreatedEffect(Vector3 position)
        {
            SimplePool.Spawn(effectDatabase.SpawnBoosterEffect, EffectContainer.Transform, position, Quaternion.identity);
        }

        public void SpawnBlastEffect(Vector3 position)
        {
            SimplePool.Spawn(effectDatabase.BlastEffect, EffectContainer.Transform, position, Quaternion.identity);
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
                ItemType.Cream => effectDatabase.CreamBreakEffect,
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

        public ExplodeEffect PlayExplodeEffect(Vector3 position)
        {
            return SimplePool.Spawn(effectDatabase.ExplodeEffect, EffectContainer.Transform, position, Quaternion.identity);
        }

        private void PlaySoundEffect(SoundEffectType soundEffect)
        {
            AudioClip sound = soundEffectDatabase.SoundEffectCollection[soundEffect];
            ItemSoundEffect effect = SimplePool.Spawn(effectDatabase.SoundEffect, EffectContainer.Transform, Vector3.zero, Quaternion.identity);
            effect.PlaySound(sound);
        }

        private async UniTask PreloadEffects()
        {
            SimplePool.PoolPreLoad(effectDatabase.SoundEffect.gameObject, 20, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
         
            SimplePool.PoolPreLoad(effectDatabase.SoundEffect.gameObject, 12, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.BlueMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.GreenMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.OrangeMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.PurpleMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.RedMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.YellowMatchEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.SpawnBoosterEffect, 9, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
                        
            SimplePool.PoolPreLoad(effectDatabase.StripedHorizontal, 3, EffectContainer.Transform);
            await UniTask.NextFrame(_token);
            
            SimplePool.PoolPreLoad(effectDatabase.StripedVertical, 3, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(targetObject.gameObject, 25, EffectContainer.Transform);
            await UniTask.NextFrame(_token);

            SimplePool.PoolPreLoad(effectDatabase.WrappedEffect, 3, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.ExplodeEffect.gameObject, 6, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.ItemSwapEffect, 4, EffectContainer.Transform);
            SimplePool.PoolPreLoad(effectDatabase.BlastEffect, 10, EffectContainer.Transform);
        }
    }
}
