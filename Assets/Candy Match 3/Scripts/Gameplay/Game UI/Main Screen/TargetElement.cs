using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.DataStructs;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class TargetElement : MonoBehaviour
    {
        [SerializeField] private TargetEnum targetType;
        [SerializeField] private Animator targetCellAnimator;

        [Space(10)]
        [SerializeField] private TMP_Text amount;
        [SerializeField] private Image targetIcon;

        [Space(10)]
        [SerializeField] private GameObject finishObject;
        [SerializeField] private GameObject failedObject;

        private TargetView _targetView;
        private TargetStats _targetStats;
        private readonly int _goalAchievedHash = Animator.StringToHash("GoalAchieved");

        public TargetEnum TargetType => targetType;

        public void UpdateTargetView(TargetView targetView)
        {
            _targetView = targetView;
            targetType = targetView.TargetType;
            targetIcon.sprite = targetView.Icon;
        }

        public void UpdateTargetCount(TargetStats stats)
        {
            _targetStats = stats;
            amount.text = $"{stats.Amount}";
            finishObject.SetActive(stats.IsCompleted);
            failedObject.SetActive(stats.IsFailed);
            amount.gameObject.SetActive(!stats.IsCompleted && !stats.IsFailed);
        }

        public void PlayTargetAnimation()
        {
            targetCellAnimator.SetTrigger(_goalAchievedHash);
        }

        public TargetView GetView()
        {
            return _targetView;
        }

        public TargetStats GetStats()
        {
            return _targetStats;
        }
    }
}
