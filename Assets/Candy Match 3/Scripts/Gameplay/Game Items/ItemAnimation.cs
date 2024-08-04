using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CandyMatch3.Scripts.Common.Constants;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemAnimation : MonoBehaviour
    {
        [SerializeField] private Animator itemAnimator;
        [SerializeField] private SpriteRenderer itemRenderer;

        [Header("Movement")]
        [SerializeField] private float swapDuration = 0.3f;
        [SerializeField] private Ease moveEase = Ease.OutQuad;

        [Header("Fading")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        private int _originalSortingOrder;

        private Tweener _moveTween;
        private Tweener _swapTween;

        private CancellationToken _destroyToken;

        private void Awake()
        {
            _originalSortingOrder = itemRenderer.sortingOrder;
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public UniTask MoveTo(Vector3 toPosition, float duration)
        {
            _moveTween ??= CreateMoveTween(toPosition, duration, moveEase);
            _moveTween.ChangeStartValue(transform.position);
            _moveTween.ChangeEndValue(toPosition);
            _moveTween.Play();

            return UniTask.Delay(TimeSpan.FromSeconds(_moveTween.Duration()), cancellationToken: _destroyToken);
        }

        public void JumpDown(float amptitude)
        {
            float magnitude = Mathf.Clamp01(1f - amptitude);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.JumpDownHash);
            itemAnimator.SetFloat(ItemAnimationHashKeys.AmptitudeHash, magnitude);
        }

        private Tweener CreateMoveTween(Vector3 toPosition, float duration, Ease ease)
        {
            return transform.DOMove(toPosition, duration).SetEase(ease).SetAutoKill(false);
        }

        private void SwapItemLayer(bool isPrioritized)
        {
            itemRenderer.sortingOrder = isPrioritized ? _originalSortingOrder + 1 : _originalSortingOrder;
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _swapTween?.Kill();
        }
    }
}
