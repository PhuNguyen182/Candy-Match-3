using R3;
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

        private void Awake()
        {
            nextButton.onClick.AddListener(OnNextClicked);
            scoreTween.BindInt(_reactiveScore, ShowScore);
            _reactiveScore.Value = 0;
        }

        public void SetBackground(GameObject background)
        {
            _background = background;
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

        private void OnNextClicked()
        {

        }

        private async UniTask Close()
        {
            await UniTask.CompletedTask;
        }
    }
}
