using R3;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Effects.Tweens;
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

        private GameObject _background;
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

        public void SetBackground(GameObject background)
        {
            _background = background;
        }

        public UniTask ShowWinGame()
        {
            _source = new();
            gameObject.SetActive(true);
            return _source.Task;
        }

        public void UpdateScore(int score)
        {
            _reactiveScore.Value = score;
        }

        public void UpdateLevel()
        {
            //levelText.text = "Current Level in Game Data";
        }

        private void ShowScore(int score)
        {
            scoreText.text = $"Your score: <color=#B83555>{score:N1, ru-RU}</color>";
        }

        private async UniTask OnNextClicked()
        {
            await Close();
            _source.TrySetResult();
            gameObject.SetActive(false);
        }

        private async UniTask Close()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}
