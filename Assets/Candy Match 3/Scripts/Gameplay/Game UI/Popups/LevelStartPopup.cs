using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Gameplay.Models;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UIElements;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class LevelStartPopup : MonoBehaviour
    {
        [SerializeField] private TargetElement targetElement;
        [SerializeField] private Transform targetContainer;
        [SerializeField] private TMP_Text requiredScoreText;
        [SerializeField] private GameObject requireScoreObject;
        [SerializeField] private TargetDatabase targetDatabase;
        [SerializeField] private FadeBackground background;

        private CancellationToken _token;
        private List<TargetElement> _targets = new();

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
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

        public async UniTask ClosePopup()
        {
            background.ShowBackground(false);
            await UniTask.Delay(TimeSpan.FromSeconds(1.667f), cancellationToken: _token);
            gameObject.SetActive(false);
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
