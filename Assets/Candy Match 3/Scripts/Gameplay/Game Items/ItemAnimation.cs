using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Constants;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemAnimation : MonoBehaviour
    {
        [SerializeField] private Animator itemAnimator;
        [SerializeField] private SpriteRenderer itemRenderer;
        [SerializeField] private AnimationCurve fallenCurve;

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
            _moveTween.ChangeValues(transform.position, toPosition, duration);
            _moveTween.Play();

            TimeSpan totalDuration = TimeSpan.FromSeconds(_moveTween.Duration());
            return UniTask.Delay(totalDuration , cancellationToken: _destroyToken);
        }

        public void JumpDown(float amptitude)
        {
            float magnitude = Mathf.Clamp01(amptitude);
            float smoothedMagnitude = fallenCurve.Evaluate(magnitude);
            itemAnimator.SetFloat(ItemAnimationHashKeys.AmptitudeHash, smoothedMagnitude);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.JumpDownHash);
        }

        public void BounceTap()
        {
            itemAnimator.SetTrigger(ItemAnimationHashKeys.BounceHash);
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
