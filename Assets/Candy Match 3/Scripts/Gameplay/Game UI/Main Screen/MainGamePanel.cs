using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.DataStructs;
using TMPro;
using CandyMatch3.Scripts.Gameplay.Models;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class MainGamePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text moveCount;
        [SerializeField] private ScoreViews scoreViews;
        [SerializeField] private TargetViews targetViews;

        public Dictionary<TargetEnum, TargetElement> TargetElements => targetViews.TargetElements;

        public void InitTargets(List<TargetView> targetView, List<TargetStats> targetStat)
        {
            targetViews.Init(targetView, targetStat);
        }

        public void UpdateMove(int move)
        {
            moveCount.text = $"{move}";
        }

        public void InitScore(ScoreRule scoreRule)
        {
            scoreViews.Init(scoreRule);
        }

        public void UpdateScore(int score)
        {
            scoreViews.ShowScore(score);
        }

        public void UpdateTarget(TargetEnum targetEnum, TargetStats stats)
        {
            targetViews.UpdateElement(targetEnum, stats);
        }
    }
}
