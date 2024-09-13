using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class LevelStartPopup : MonoBehaviour
    {
        [SerializeField] private TargetElement targetElement;
        [SerializeField] private Transform targetContainer;
        [SerializeField] private TMP_Text requiredScoreText;
        [SerializeField] private GameObject requireScoreObject;

        public void ShowTarget(List<TargetElement> remainTargets)
        {
            for (int i = 0; i < remainTargets.Count; i++)
            {
                TargetView targetView = remainTargets[i].GetView();
                TargetStats targetStats = remainTargets[i].GetStats();

                TargetElement target = SimplePool.Spawn(targetElement, targetContainer
                                        , targetContainer.position, Quaternion.identity);
                target.transform.localScale = Vector3.one;

                target.UpdateTargetView(targetView);
                target.UpdateTargetCount(targetStats);
            }
        }
    }
}
