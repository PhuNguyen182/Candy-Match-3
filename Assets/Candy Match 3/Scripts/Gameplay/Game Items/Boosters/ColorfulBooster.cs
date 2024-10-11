using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public class ColorfulBooster : BaseBoosterItem, IItemSuggest
    {
        [Header("Effects")]
        [SerializeField] private GameObject colorfulEffect;
        [SerializeField] private GameObject chargingEffect;

        private GameObject _chargeEffect;

        public bool IsSuggesting { get; set; }

        public override async UniTask Activate()
        {
            _chargeEffect = SimplePool.Spawn(chargingEffect, transform, transform.position, Quaternion.identity);
            await UniTask.CompletedTask;
        }

        public override void ResetItem()
        {
            base.ResetItem();
            SetMatchable(false);
            OnItemReset();
        }

        public override void Explode()
        {
            if (_chargeEffect != null)
            {
                _chargeEffect.transform.SetParent(EffectContainer.Transform);
                SimplePool.Despawn(_chargeEffect);
            }

            EffectManager.Instance.PlaySoundEffect(SoundEffectType.ColorBomb);
            SimplePool.Spawn(colorfulEffect, EffectContainer.Transform, WorldPosition, Quaternion.identity);
        }

        public void Highlight(bool isActive)
        {
            IsSuggesting = isActive;
            itemAnimation.ToggleSuggest(isActive);
        }

        public override void ReleaseItem()
        {
            itemAnimation.ToggleSuggest(false);
            SimplePool.Despawn(this.gameObject);
        }

        private void OnItemReset()
        {
            IsActivated = false;
        }
    }
}
