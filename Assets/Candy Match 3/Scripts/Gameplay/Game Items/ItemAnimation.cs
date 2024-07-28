using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemAnimation : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        [Header("Fading")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        private Tweener _moveTween;
        private Tweener _swapTween;

        private CancellationToken _destroyToken;

        private void Awake()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public UniTask MoveTo(Vector3 toPosition)
        {
            _moveTween ??= CreateMoveTween(toPosition, moveDuration, moveEase);
            _moveTween.ChangeStartValue(transform.position);
            _moveTween.ChangeEndValue(toPosition);
            _moveTween.Play();

            return UniTask.Delay(TimeSpan.FromSeconds(_moveTween.Duration()), cancellationToken: _destroyToken);
        }

        private Tweener CreateMoveTween(Vector3 toPosition, float duration, Ease ease)
        {
            return transform.DOMove(toPosition, duration).SetEase(ease).SetAutoKill(false);
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _swapTween?.Kill();
        }
    }
}
