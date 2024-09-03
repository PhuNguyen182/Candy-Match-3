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
        [SerializeField] private ItemGraphics itemGraphics;
        [SerializeField] private SpriteRenderer itemRenderer;
        [SerializeField] private AnimationCurve fallenCurve;
        [SerializeField] private AnimationCurve movingCurve;

        [Header("Movement")]
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private Ease bounceEase = Ease.OutQuad;

        [Header("Fading")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;

        [Header("Hightlight")]
        [SerializeField] private float highlightDuration = 1f;
        [SerializeField] private AnimationCurve highlightEase;

        private int _originalSortingOrder;

        private Tweener _moveTween;
        private Tweener _bounceMoveTween;
        private Tweener _swapTween;

        private Coroutine _highlightCoroutine;
        private CancellationToken _destroyToken;

        private void Awake()
        {
            _originalSortingOrder = itemRenderer.sortingOrder;
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public UniTask MoveTo(Vector3 toPosition, float duration)
        {
            _moveTween ??= CreateMoveTween(toPosition, duration);
            _moveTween.ChangeValues(transform.position, toPosition, duration);
            _moveTween.Play();

            TimeSpan totalDuration = TimeSpan.FromSeconds(_moveTween.Duration());
            return UniTask.Delay(totalDuration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            SwapItemLayer(isMoveFirst);
            _moveTween ??= CreateMoveTween(position, duration);
            _moveTween.ChangeValues(transform.position, position, duration);
            _moveTween.Play();
            SwapItemLayer(false);

            TimeSpan totalDuration = TimeSpan.FromSeconds(_moveTween.Duration());
            return UniTask.Delay(totalDuration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
        }

        public UniTask BounceMove(Vector3 position)
        {
            _bounceMoveTween ??= CreateMoveBounceTween(position);
            _bounceMoveTween.ChangeStartValue(transform.position);
            _bounceMoveTween.ChangeEndValue(position);

            _bounceMoveTween.Rewind();
            _bounceMoveTween.Play();

            TimeSpan duration = TimeSpan.FromSeconds(_bounceMoveTween.Duration());
            return UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
        }

        public void JumpDown(float amptitude)
        {
            float magnitude = Mathf.Clamp01(amptitude);
            float smoothedMagnitude = fallenCurve.Evaluate(magnitude);
            itemAnimator.SetFloat(ItemAnimationHashKeys.AmptitudeHash, smoothedMagnitude);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.JumpDownHash);
        }

        public UniTask DisappearOnMatch(bool isMatch)
        {
            if(isMatch)
                itemAnimator.SetTrigger(ItemAnimationHashKeys.MatchHash);
            else
                itemAnimator.ResetTrigger(ItemAnimationHashKeys.MatchHash);

            TimeSpan duration = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            return UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
        }

        public void BounceTap()
        {
            itemAnimator.SetTrigger(ItemAnimationHashKeys.BounceHash);
        }

        public void ToggleSuggest(bool isActive)
        {
            if (isActive)
            {
                if (_highlightCoroutine != null)
                    StopCoroutine(_highlightCoroutine);

                _highlightCoroutine = StartCoroutine(Highlight());
            }
            
            else
            {
                if (_highlightCoroutine != null)
                    StopCoroutine(_highlightCoroutine);

                itemGraphics.SetFloatRendererProperty(ItemShaderProperties.SuggestHighlight, 0);
            }

            itemAnimator.SetBool(ItemAnimationHashKeys.SuggestHash, isActive);
        }

        private Tweener CreateMoveBounceTween(Vector3 position)
        {
            return transform.DOMove(position, bounceDuration)
                            .SetEase(bounceEase).SetLoops(2, LoopType.Yoyo)
                            .SetAutoKill(false);
        }

        private Tweener CreateMoveTween(Vector3 toPosition, float duration)
        {
            return transform.DOMove(toPosition, duration).SetEase(movingCurve).SetAutoKill(false);
        }

        private IEnumerator Highlight()
        {
            float ratio = 0;
            float elapsedTime = 0;

            while (true)
            {
                elapsedTime += Time.deltaTime;
                ratio = highlightEase.Evaluate(elapsedTime / highlightDuration);
                itemGraphics.SetFloatRendererProperty(ItemShaderProperties.SuggestHighlight, ratio);

                if (elapsedTime > highlightDuration)
                    elapsedTime = 0;

                yield return null;
            }
        }

        private void SwapItemLayer(bool isPrioritized)
        {
            itemRenderer.sortingOrder = isPrioritized ? _originalSortingOrder + 1 : _originalSortingOrder;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            itemGraphics ??= GetComponent<ItemGraphics>();
        }
#endif

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _bounceMoveTween?.Kill();
            _swapTween?.Kill();
            ToggleSuggest(false);
        }
    }
}
