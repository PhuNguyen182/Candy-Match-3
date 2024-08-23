using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public class ColorfulBooster : BaseBoosterItem
    {
        [Header("Effects")]
        [SerializeField] private GameObject colorfulEffect;

        private GameObject _effect;

        public override async UniTask Activate()
        {
            if(colorfulEffect != null)
                _effect = SimplePool.Spawn(colorfulEffect, transform, transform.position, Quaternion.identity);
            
            await UniTask.CompletedTask;
        }

        public override void Explode()
        {
            if (_effect != null)
            {
                SimplePool.Despawn(_effect);
                _effect.transform.SetParent(EffectContainer.Transform);
            }
        }

        public override void ReleaseItem()
        {
            SimplePool.Despawn(this.gameObject);
        }
    }
}
