using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.GameManagers;
using CandyMatch3.Scripts.Mainhome.UI.Popups;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Constants;
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

        private int _heart = 0;
        private TimeSpan _lifeTime;

        private void Awake()
        {
            PreloadPopup();
            lifeButton.onClick.AddListener(() => OpenLifePopup().Forget());
        }

        private void Update()
        {
            UpdateLives();
        }

        private void PreloadPopup()
        {
            BuyLivesPopup.PreloadFromAddress(CommonPopupPaths.LivesPopupPath).Forget();
        }

        private async UniTask OpenLifePopup()
        {
            if (_heart >= 5)
                await OnFullLives();

            else
            {
                if (_lifeTime.TotalSeconds <= 1.5f)
                    await OnFullLives();
                
                else
                    BuyLivesPopup.CreateFromAddress(CommonPopupPaths.LivesPopupPath).Forget();
            }
        }

        private async UniTask OnFullLives()
        {
            var alert = await AlertPopup.CreateFromAddress(CommonPopupPaths.AlertPopupPath);
            alert.SetMessage("Your lives is full!\nLet's play game!");
            alert.OnClose = () =>
            {
                OpenPlayGamePopup().Forget();
            };
        }

        private async UniTask OpenPlayGamePopup()
        {
            int level = GameDataManager.Instance.GetCurrentLevel();
            var startGamePopup = await StartGamePopup.CreateFromAddress(CommonPopupPaths.StartGamePopupPath);

            await startGamePopup.SetLevelInfo(new LevelBoxData
            {
                Level = level,
                Stars = 0
            });
        }

        private void UpdateLives()
        {
            _heart = GameDataManager.Instance.GetResource(GameResourceType.Life);
            _lifeTime = GameManager.Instance.HeartTimer.HeartTimeDiff;

            UpdateLives(_heart);
            UpdateTime(_heart, _lifeTime);
        }

        private void UpdateTime(int heart, TimeSpan time)
        {
            if (heart >= 5)
                timeCounter.text = "Full";

            else
                timeCounter.text = time.TotalSeconds > 3600 ? $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}"
                                                            : $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void UpdateLives(int lives)
        {
            lifeCounter.text = $"{lives}";
        }

        private void OnDestroy()
        {
            BuyLivesPopup.Release();
        }
    }
}
