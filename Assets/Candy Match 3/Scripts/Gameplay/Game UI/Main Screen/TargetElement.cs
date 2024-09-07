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

        [Space(10)]
        [SerializeField] private TMP_Text amount;
        [SerializeField] private Image targetIcon;

        [Space(10)]
        [SerializeField] private GameObject finishObject;
        [SerializeField] private GameObject failedObject;

        public TargetEnum TargetType => targetType;

        public void UpdateTargetView(TargetView targetView)
        {
            targetType = targetView.TargetType;
            targetIcon.sprite = targetView.Icon;
        }

        public void UpdateTargetCount(TargetStats stats)
        {
            amount.text = $"{stats.Amount}";
            finishObject.SetActive(stats.IsCompleted);
            failedObject.SetActive(stats.IsFailed);
            amount.gameObject.SetActive(!stats.IsCompleted && !stats.IsFailed);
        }

        public void PlayTargetAnimation()
        {

        }
    }
}
