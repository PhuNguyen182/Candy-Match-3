using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Boosters
{
    public class BombBooster : BaseBoosterItem
    {
        public override async UniTask Activate()
        {
            await UniTask.CompletedTask;
        }

        public override void Explode()
        {
            
        }
    }
}
