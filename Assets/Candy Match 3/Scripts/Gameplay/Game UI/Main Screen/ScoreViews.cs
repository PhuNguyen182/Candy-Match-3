using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Gameplay.Models;
using GlobalScripts.Effects.Tweens;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class ScoreViews : MonoBehaviour
    {
        [SerializeField] private Image scoreFill;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text targetScore;
        [SerializeField] private StarStreak[] starStreaks;
        [SerializeField] private TweenValueEffect starTween;

        private float _streak1 = 0;
        private float _streak2 = 0;
        private float _streak3 = 0;

        private ScoreRule _scoreRule;
        private ReactiveProperty<int> _reactiveScore = new(0);

        private void Start()
        {
            starTween.BindInt(_reactiveScore, UpdateScore);
            _reactiveScore.Value = 0;
        }

        public void Init(ScoreRule scoreRule)
        {
            _scoreRule = scoreRule;
            _streak1 = (float)scoreRule.Star1Score / scoreRule.MaxScore;
            _streak2 = (float)scoreRule.Star2Score / scoreRule.MaxScore;
            _streak3 = (float)scoreRule.Star3Score / scoreRule.MaxScore;

            starStreaks[0].SetStarActive(false);
            starStreaks[1].SetStarActive(false);
            starStreaks[2].SetStarActive(false);
        }

        private void UpdateScore(int score)
        {
            scoreText.text = $"{score}";
            float scoreFill = (float)score / _scoreRule.MaxScore;
            ShowScoreFill(scoreFill);
        }

        private void ShowScoreFill(float fillAmount)
        {
            StarFill(fillAmount);

            if (fillAmount >= _streak3)
                starStreaks[2].SetStarActive(true);

            else if (fillAmount >= _streak2)
                starStreaks[1].SetStarActive(true);

            else if (fillAmount >= _streak1)
                starStreaks[0].SetStarActive(true);
        }

        public void ShowScore(int score)
        {
            _reactiveScore.Value = score;
        }

        private void StarFill(float fillAmount)
        {
            float fill = Mathf.Clamp(fillAmount, 0f, 1f);
            scoreFill.fillAmount = fill;
        }
    }
}
