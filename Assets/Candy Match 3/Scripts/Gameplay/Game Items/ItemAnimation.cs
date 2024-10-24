using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Constants;
using GlobalScripts.UpdateHandlerPattern;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.GameItems
{
    public class ItemAnimation : MonoBehaviour
    {
        [SerializeField] private Animator itemAnimator;
        [SerializeField] private ItemGraphics itemGraphics;
        [SerializeField] private SpriteRenderer itemRenderer;

        [Header("Movement Ease")]
        [SerializeField] private Ease matchItemEase;
        [SerializeField] private AnimationCurve fallenCurve;
        [SerializeField] private AnimationCurve movingCurve;

        [Header("Bounce Movement")]
        [SerializeField] private float bounceDuration = 0.3f;
        [SerializeField] private Ease bounceEase = Ease.OutQuad;

        [Header("Hightlight")]
        [SerializeField] private float glowLightDuration = 1f;
        [SerializeField] private float highlightDuration = 1f;
        [SerializeField] private float boosterDuration = 1f;
        [SerializeField] private AnimationCurve highlightEase;
        [SerializeField] private AnimationCurve glowlightEase;
        [SerializeField] private AnimationCurve boosterEase;

        private bool _hasBeenSuggested;
        private int _originalSortingOrder;

        private Tweener _moveTween;
        private Tweener _bounceMoveTween;
        private Tweener _matchTween;

        private Coroutine _highlightCoroutine;
        private Coroutine _glowlightCoroutine;
        private Coroutine _glowBoosterCoroutine;
        private CancellationToken _destroyToken;

        public Animator ItemAnimator => itemAnimator;

        private void Awake()
        {
            _originalSortingOrder = itemRenderer.sortingOrder;
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public UniTask MatchTo(Vector3 toPosition, float duration)
        {
            ChangeItemLayer(true);
            _matchTween ??= CreateMatchTween(toPosition, duration);
            _matchTween.ChangeValues(transform.position, toPosition, duration);
            _matchTween.Play();

            TimeSpan totalDuration = TimeSpan.FromSeconds(_matchTween.Duration());
            return UniTask.Delay(totalDuration, false, PlayerLoopTiming.FixedUpdate, _destroyToken)
                          .ContinueWith(() => ChangeItemLayer(false));
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
            ChangeItemLayer(isMoveFirst);
            _moveTween ??= CreateMoveTween(position, duration);
            _moveTween.ChangeValues(transform.position, position, duration);
            _moveTween.Play();

            TimeSpan totalDuration = TimeSpan.FromSeconds(_moveTween.Duration());
            return UniTask.Delay(totalDuration, false, PlayerLoopTiming.FixedUpdate, _destroyToken)
                          .ContinueWith(() => ChangeItemLayer(false));
        }

        public UniTask BounceMove(Vector3 position)
        {
            _bounceMoveTween ??= CreateMoveBounceTween(position);
            _bounceMoveTween.ChangeStartValue(itemRenderer.transform.localPosition);
            _bounceMoveTween.ChangeEndValue(position);

            _bounceMoveTween.Rewind();
            _bounceMoveTween.Play();

            TimeSpan duration = TimeSpan.FromSeconds(_bounceMoveTween.Duration());
            return UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _destroyToken)
                          .ContinueWith(() => itemRenderer.transform.localPosition = Vector3.zero);
        }

        public void JumpDown(float amptitude)
        {
            float magnitude = Mathf.Clamp01(amptitude);
            float smoothedMagnitude = fallenCurve.Evaluate(magnitude);
            itemAnimator.SetFloat(ItemAnimationHashKeys.AmptitudeHash, smoothedMagnitude);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.JumpDownHash);
        }

        public void Nudge(Vector3Int direction)
        {
            itemAnimator.SetTrigger(ItemAnimationHashKeys.NudgeHash);
            itemAnimator.SetFloat(ItemAnimationHashKeys.NudgeXHash, direction.x);
            itemAnimator.SetFloat(ItemAnimationHashKeys.NudgeYHash, direction.y);
        }

        public void TriggerVibrate(int stage = 0)
        {
            PlayBoosterTrigger();
            itemAnimator.SetTrigger(ItemAnimationHashKeys.ExplodeHash);
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

        public async UniTask PlayStripedWrapped()
        {
            PlayGlowLightEffect();
            ChangeVisibleMask(true);
            ChangeItemLayer(true, 3);

            itemAnimator.ResetTrigger(ItemAnimationHashKeys.ComboStripedWrappedHash);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.ComboStripedWrappedHash);
            TimeSpan duration = TimeSpan.FromSeconds(Match3Constants.ComboStripedWrappedDelay);
            await UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
            ChangeItemLayer(false);
        }

        public async UniTask PlayDoubleWrapped(int direction, bool isFirst)
        {
            PlayGlowLightEffect();
            ChangeVisibleMask(true);
            ChangeItemLayer(true, 3);
            itemAnimator.SetBool(ItemAnimationHashKeys.IsFirstHash, isFirst);
            itemAnimator.SetInteger(ItemAnimationHashKeys.DirectionHash, direction);
            itemAnimator.SetTrigger(ItemAnimationHashKeys.ComboDoubleWrappedHash);

            TimeSpan duration = TimeSpan.FromSeconds(Match3Constants.ComboDoubleWrappedDelay);
            await UniTask.Delay(duration, false, PlayerLoopTiming.FixedUpdate, _destroyToken);
            ChangeItemLayer(false);
        }

        public void BounceTap()
        {
            itemAnimator.SetTrigger(ItemAnimationHashKeys.BounceHash);
        }

        public void ToggleSuggest(bool isActive)
        {
            if (!_hasBeenSuggested && !isActive)
                return;

            _hasBeenSuggested = isActive;
            ChangeVisibleMask(isActive);
            itemAnimator.ResetTrigger(ItemAnimationHashKeys.NudgeHash);
            itemAnimator.SetBool(ItemAnimationHashKeys.SuggestHash, isActive);

            if (isActive)
            {
                ClearSuggestEffect();
                PlaySuggestEffect();
            }
            
            else
            {
                itemGraphics.SetFloatRendererProperty(ItemShaderProperties.HighlightAmount, 0);
                ClearSuggestEffect(); // Should be place here to prevent destroy null reference
            }
        }

        public void ChangeVisibleMask(bool enable)
        {
            itemGraphics.ChangeMaskInteraction(enable);
        }

        public void Transform()
        {
            itemAnimator.SetTrigger(ItemAnimationHashKeys.TransformHash);
        }

        public void ResetItem()
        {
            itemRenderer.transform.localPosition = Vector3.zero;
        }

        private Tweener CreateMoveBounceTween(Vector3 position)
        {
            return itemRenderer.transform.DOLocalMove(position, bounceDuration)
                               .SetEase(bounceEase).SetLoops(2, LoopType.Yoyo)
                               .SetAutoKill(false);
        }

        private Tweener CreateMoveTween(Vector3 toPosition, float duration)
        {
            return transform.DOMove(toPosition, duration).SetEase(movingCurve).SetAutoKill(false);
        }

        private Tweener CreateMatchTween(Vector3 toPosition, float duration)
        {
            return transform.DOMove(toPosition, duration).SetEase(matchItemEase).SetAutoKill(false);
        }

        private IEnumerator Highlight(float duration, AnimationCurve ease, bool canStop = false)
        {
            float ratio = 0;
            float elapsedTime = 0;

            while (true)
            {
                elapsedTime += Time.deltaTime;
                ratio = ease.Evaluate(elapsedTime / duration);
                itemGraphics.SetFloatRendererProperty(ItemShaderProperties.HighlightAmount, ratio);

                if (elapsedTime > duration && !canStop)
                    elapsedTime = 0;

                yield return null;
            }
        }

        private void PlayBoosterTrigger()
        {
            if (isActiveAndEnabled)
                _glowBoosterCoroutine = StartCoroutine(Highlight(boosterDuration, boosterEase, false));
        }

        private void PlayGlowLightEffect()
        {
            _glowlightCoroutine = StartCoroutine(Highlight(glowLightDuration, glowlightEase, true));
        }

        private void PlaySuggestEffect()
        {
            _highlightCoroutine = StartCoroutine(Highlight(highlightDuration, highlightEase));
        }

        private void ClearSuggestEffect()
        {
            if (_highlightCoroutine != null)
                StopCoroutine(_highlightCoroutine);
        }

        private void ChangeItemLayer(bool isPrioritized, int priorityAmount = 1)
        {
            itemRenderer.sortingOrder = isPrioritized ? _originalSortingOrder + priorityAmount : _originalSortingOrder;
        }

        private void StopAllEffects()
        {
            ToggleSuggest(false);
            itemGraphics.SetFloatRendererProperty(ItemShaderProperties.HighlightAmount, 0);

            if (_glowlightCoroutine != null)
                StopCoroutine(_glowlightCoroutine);

            if (_glowBoosterCoroutine != null)
                StopCoroutine(_glowBoosterCoroutine);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            itemGraphics ??= GetComponent<ItemGraphics>();
        }
#endif

        private void OnDisable()
        {
            StopAllEffects();
            ChangeVisibleMask(false);
            itemRenderer.transform.localPosition = Vector3.zero;
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _bounceMoveTween?.Kill();
            _matchTween?.Kill();
        }
    }
}
