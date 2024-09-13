using System.Collections;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Enums;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class TargetViews : MonoBehaviour
    {
        [SerializeField] private Transform targetContainer;
        [SerializeField] private TargetElement targetElement;

        private Dictionary<TargetEnum, TargetElement> _targetElements;

        public Dictionary<TargetEnum, TargetElement> TargetElements => _targetElements;

        public void Init(List<TargetView> targetViews, List<TargetStats> targetStats)
        {
            _targetElements = new();

            for (int i = 0; i < targetViews.Count; i++)
            {
                TargetElement target = SimplePool.Spawn(targetElement, targetContainer
                                        , targetContainer.position, Quaternion.identity);
                
                target.transform.localScale = Vector3.one;
                target.UpdateTargetView(targetViews[i]);
                target.UpdateTargetCount(targetStats[i]);

                _targetElements.Add(target.TargetType, target);
            }
        }

        public void UpdateElement(TargetEnum targetEnum, TargetStats stats)
        {
            var targetCell = _targetElements[targetEnum];
            targetCell.UpdateTargetCount(stats);
        }
    }
}
