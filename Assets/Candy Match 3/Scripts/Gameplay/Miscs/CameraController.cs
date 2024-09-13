using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace CandyMatch3.Scripts.Gameplay.Miscs
{
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private Ease moveEase = Ease.InOutSine;

        [Space(10)]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private SpriteRenderer background;

        private const float DefaultCameraSize = 7f;
        private const float DefaultBackgroundScale = 0.75f;
        private const float DefaultScreenRatio = 16f / 9f;

        private Tweener _moveTween;
        private Tweener _moveSlowTween;
        private CancellationToken _token;

        private void Awake()
        {
            cinemachineBrain.useGUILayout = false; // Stop generating 368B GC.Alloc
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void Start()
        {
            GameScreenCalculate();
        }

        private void GameScreenCalculate()
        {
            float currentScreenRatio = 1f / mainCamera.aspect;
            
            if (currentScreenRatio > DefaultScreenRatio)
            {
                mainCamera.orthographicSize = DefaultCameraSize * currentScreenRatio / DefaultScreenRatio;
                background.transform.localScale = Vector3.one * DefaultBackgroundScale
                                                  * currentScreenRatio / DefaultScreenRatio;
            }
        }

        public void SetPosition(Vector3 toPosition)
        {
            transform.position = toPosition;
        }

        public UniTask MoveTo(Vector3 toPosition)
        {
            _moveTween ??= CreateMoveTween(toPosition);
            _moveTween.ChangeStartValue(transform.position);
            _moveTween.ChangeEndValue(toPosition);

            _moveTween.Rewind();
            _moveTween.Play();

            return UniTask.Delay(TimeSpan.FromSeconds(_moveTween.Duration()), cancellationToken: _token);
        }
        
        public UniTask MoveToZero(Vector3 toPosition)
        {
            _moveSlowTween ??= CreateMoveSlowTween(toPosition);
            _moveSlowTween.ChangeStartValue(transform.position);
            _moveSlowTween.ChangeEndValue(toPosition);

            _moveSlowTween.Rewind();
            _moveSlowTween.Play();

            return UniTask.Delay(TimeSpan.FromSeconds(_moveSlowTween.Duration()), cancellationToken: _token);
        }

        private Tweener CreateMoveTween(Vector3 toPosition)
        {
            return transform.DOMove(toPosition, moveDuration).SetEase(moveEase).SetAutoKill(false);
        }

        private Tweener CreateMoveSlowTween(Vector3 toPosition)
        {
            float duration = toPosition.magnitude / 12f;
            return transform.DOMove(toPosition, duration).SetEase(moveEase).SetAutoKill(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            mainCamera ??= Camera.main;
        }
#endif

        private void OnDestroy()
        {
            _moveTween?.Kill();
        }
    }
}
