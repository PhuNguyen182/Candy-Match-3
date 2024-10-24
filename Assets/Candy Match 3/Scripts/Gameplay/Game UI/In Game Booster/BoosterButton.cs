using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster
{
    public class BoosterButton : MonoBehaviour
    {
        [SerializeField] private InGameBoosterType boosterType;
        [SerializeField] private Button boosterButton;
        [SerializeField] private TMP_Text boosterCount;
        [SerializeField] private GameObject boosterCounter;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private GameObject addObject;
        [SerializeField] private Canvas buttonCanvas;

        private int _count;
        private bool _isFree;
        private bool _isActive;
        private bool _isLocked;

        public InGameBoosterType BoosterType => boosterType;

        public Observable<(bool IsActive, bool IsFree)> OnClickObserver
            => boosterButton.OnClickAsObservable().Where(_ => !_isLocked)
                            .Select(_ => (_isActive, _isFree));

        public void SetButtonUsageState(bool isUsing)
        {
            SetButtonUIOverride(isUsing);
        }

        public void SetButtonUIOverride(bool isOverride)
        {
            buttonCanvas.overrideSorting = isOverride;
        }

        public void SetBoosterCount(int count)
        {
            _count = count;
            boosterCount.text = $"{count}";
            boosterCounter.SetActive(count > 0);
            addObject.gameObject.SetActive(count <= 0);
        }

        public void SetFreeState(bool isFree)
        {
            _isFree = isFree;
        }

        public void SetLockState(bool isLocked)
        {
            _isLocked = isLocked;
            lockObject.SetActive(isLocked);
        }
    }
}
