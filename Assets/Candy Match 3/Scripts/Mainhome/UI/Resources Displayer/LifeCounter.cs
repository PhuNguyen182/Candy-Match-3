using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.GameManagers;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Mainhome.UI.Popups;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;
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
            PreloadPopup();
            lifeButton.onClick.AddListener(OpenLifePopup);
        }

        private void Update()
        {
            UpdateLives();
        }

        private void PreloadPopup()
        {
            BuyLivesPopup.PreloadFromAddress(CommonPopupPaths.LivesPopupPath).Forget();
        }

        private void OpenLifePopup()
        {
            // To do: if life count is equal to 5, open start game popup, otherwise open buy heart popup
            BuyLivesPopup.CreateFromAddress(CommonPopupPaths.LivesPopupPath).Forget();
        }

        private void UpdateLives()
        {
            int heart = GameDataManager.Instance
                        .GetResource(GameResourceType.Life);
            TimeSpan time = GameManager.Instance
                            .HeartTimer.HeartTimeDiff;

            UpdateLives(heart);
            UpdateTime(heart, time);
        }

        private void UpdateTime(int heart, TimeSpan time)
        {
            if (heart >= 5)
            {
                timeCounter.text = "Full";
            }

            else
            {
                timeCounter.text = time.TotalSeconds > 3600 ? $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"
                                                            : $"{time.Minutes:D2}:{time.Seconds:D2}";
            }
        }

        private void UpdateLives(int lives)
        {
            lifeCounter.text = $"{lives}";
        }
    }
}
