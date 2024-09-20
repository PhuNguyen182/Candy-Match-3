using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public class ColorfulBooster : BaseBoosterItem, IItemSuggest
    {
        [Header("Effects")]
        [SerializeField] private GameObject colorfulEffect;

        private GameObject _effect;

        public override async UniTask Activate()
        {
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
            SimplePool.Spawn(colorfulEffect, EffectContainer.Transform, WorldPosition, Quaternion.identity);
        }

        public void Highlight(bool isActive)
        {
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
