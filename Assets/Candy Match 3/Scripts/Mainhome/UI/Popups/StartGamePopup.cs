using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.Models;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Popups
{
    public class StartGamePopup : BasePopup<StartGamePopup>
    {
        [SerializeField] private Animator animator;
        [SerializeField] private TargetElement targetElement;
        [SerializeField] private TargetDatabase targetDatabase;
        [SerializeField] private Transform targetContainer;

        [Space(10)]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;

        [Space(10)]
        [SerializeField] private GameObject[] stars;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");
        private List<TargetElement> _targets = new();

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();

            playButton.onClick.AddListener(PlayGame);
            closeButton.onClick.AddListener(() => CloseAsync().Forget());
        }

        public void ShowStars(int star)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                bool active = i + 1 <= star;
                stars[i].SetActive(active);
            }
        }

        private void PlayGame()
        {

        }

        private async UniTask CloseAsync()
        {
            animator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            base.Close();
        }

        public void InitLevelTarget(LevelModel levelModel)
        {
            List<LevelTargetData> _levelTargetDatas = levelModel.LevelTargetData;

            List<TargetView> targetViews = new();
            List<TargetStats> targetStats = new();

            for (int i = 0; i < _levelTargetDatas.Count; i++)
            {
                int amount = _levelTargetDatas[i].DataValue.TargetAmount;
                TargetEnum targetType = _levelTargetDatas[i].DataValue.Target;
                Sprite icon = targetDatabase.GetTargetIcon(targetType);

                TargetView targetView = new TargetView
                {
                    TargetType = targetType,
                    Icon = icon
                };

                TargetStats targetStat = new TargetStats
                {
                    Amount = amount,
                };

                targetViews.Add(targetView);
                targetStats.Add(targetStat);
            }

            Init(targetViews, targetStats);
        }

        private void Init(List<TargetView> targetViews, List<TargetStats> targetStats)
        {
            ClearTargets();

            for (int i = 0; i < targetViews.Count; i++)
            {
                TargetElement target = SimplePool.Spawn(targetElement, targetContainer
                                        , targetContainer.position, Quaternion.identity);

                target.transform.localScale = Vector3.one;
                target.UpdateTargetView(targetViews[i]);
                target.UpdateTargetCount(targetStats[i]);

                _targets.Add(target);
            }
        }

        private void ClearTargets()
        {
            if (_targets.Count <= 0)
                return;

            for (int i = 0; i < _targets.Count; i++)
            {
                SimplePool.Despawn(_targets[i].gameObject);
            }

            _targets.Clear();
        }
    }
}
