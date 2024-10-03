using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
using CandyMatch3.Scripts.Common.SingleConfigs;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class WinGamePanel : MonoBehaviour
    {
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TweenValueEffect scoreTween;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject[] stars;

        private int _score;
        private int _stars;

        private ReactiveProperty<int> _reactiveScore = new(0);
        private readonly int _closeHash = Animator.StringToHash("Close");
        
        private CancellationToken _token;
        private UniTaskCompletionSource _source;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            nextButton.onClick.AddListener(() => OnNextClicked().Forget());
            scoreTween.BindInt(_reactiveScore, ShowScore);
            _reactiveScore.Value = 0;
        }

        private void Start()
        {
            UpdateLevel();
        }

        public UniTask ShowWinGame()
        {
            _source = new();
            canvasGroup.interactable = false;
            gameObject.SetActive(true);
            UpdateScore().Forget();
            return _source.Task;
        }

        public void SetScoreAndStars(int score, int stars)
        {
            (_score, _stars) = (score, stars);
        }

        private async UniTask UpdateStars(int star)
        {
            if (star == 0)
                return;

            for (int i = 0; i < stars.Length; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.15f), cancellationToken: _token);

                bool isActive = i + 1 <= star;
                stars[i].SetActive(isActive);
            }
        }

        private async UniTask UpdateScore()
        {
            _reactiveScore.Value = _score;
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: _token);
            await UpdateStars(_stars);

            canvasGroup.interactable = true;
        }

        private void UpdateLevel()
        {
            int level = PlayGameConfig.Current.Level;
            levelText.text = $"Level {level}";
        }

        private void ShowScore(int score)
        {
            scoreText.text = $"Your score: <color=#B83555>{score}</color>";
        }

        private async UniTask OnNextClicked()
        {
            await CloseAnimation();
            _source.TrySetResult();
            gameObject.SetActive(false);
        }

        private async UniTask CloseAnimation()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
