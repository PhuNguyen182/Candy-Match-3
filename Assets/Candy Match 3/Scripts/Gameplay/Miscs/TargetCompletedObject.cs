using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Miscs
{
    public class TargetCompletedObject : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer itemRenderer;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public void SetItemIcon(Sprite sprite)
        {
            itemRenderer.sprite = sprite;
        }

        public async UniTask MoveToTarget(Vector3 destination)
        {
            await UniTask.CompletedTask;
        }
    }
}
