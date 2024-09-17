using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.ResourcesDisplayer
{
    public class LifeCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text lifeCounter;
        [SerializeField] private TMP_Text timeCounter;
        [SerializeField] private Button lifeButton;

        private void Awake()
        {
            lifeButton.onClick.AddListener(OpenLifePopup);
        }

        private void OpenLifePopup()
        {

        }

        private void UpdateTime(TimeSpan time)
        {
            if (time.TotalSeconds > 3600)
                timeCounter.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                timeCounter.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        public void UpdateLives(int lives)
        {
            lifeCounter.text = $"{lives}";
        }
    }
}
