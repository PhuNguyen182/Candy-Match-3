using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.GameData.Constants;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Mainhome.Managers;
using CandyMatch3.Scripts.Gameplay.Models;
using GlobalScripts.SceneUtils;
using Cysharp.Threading.Tasks;
using GlobalScripts.Audios;
using Newtonsoft.Json;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.UI.Popups
{
    public class StartGamePopup : BasePopup<StartGamePopup>
    {
        [SerializeField] private Animator animator;
        [SerializeField] private TargetElement targetElement;
        [SerializeField] private TargetDatabase targetDatabase;
        [SerializeField] private FadeBackground background;
        [SerializeField] private Transform targetContainer;

        [Space(10)]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button closeButton;

        [Space(10)]
        [SerializeField] private GameObject[] stars;

        private int _level;
        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private LevelModel _levelModel;
        private List<TargetElement> _targets = new();

        protected override void OnAwake()
        {
            targetDatabase.Initialize();
            _token = this.GetCancellationTokenOnDestroy();

            playButton.onClick.AddListener(() => PlayGame().Forget());
            closeButton.onClick.AddListener(() => CloseAsync().Forget());
        }

        protected override void DoAppear()
        {
            if (!IsPreload)
                AudioManager.Instance.PlaySoundEffect(SoundEffectType.PopupOpen);

            MainhomeManager.Instance?.SetInputActive(false);
            background.ShowBackground(true);
        }

        public async UniTask SetLevelInfo(LevelBoxData level)
        {
            ShowStars(level.Stars);
            _level = level.Level;
            levelText.text = $"Level {level.Level}";
            string levelData = await LevelPlayInfo.GetLevelData(level.Level);

            if (!string.IsNullOrEmpty(levelData))
            {
                using (StringReader streamReader = new(levelData))
                {
                    using (JsonReader jsonReader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer jsonSerializer = new();
                        _levelModel = jsonSerializer.Deserialize<LevelModel>(jsonReader);
                    }
                }

                InitLevelTarget(_levelModel);
            }
        }

        private void ShowStars(int star)
        {
            for (int i = 0; i < stars.Length; i++)
            {
                bool active = i + 1 <= star;
                stars[i].SetActive(active);
            }
        }

        private async UniTask PlayGame()
        {
            int lives = GameDataManager.Instance.GetResource(GameResourceType.Life);

            if (lives > 0)
            {
                if (_levelModel == null)
                    return;

                PlayGameConfig.Current = new()
                {
                    Level = _level,
                    IsTestMode = false,
                    LevelModel = _levelModel
                };

                GameDataManager.Instance.SpendResource(GameResourceType.Life, 1);
                if(lives >= GameDataConstants.MaxLives)
                    GameDataManager.Instance.SaveHeartTime(DateTime.Now);
                
                await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: _token);
                await SceneBridge.LoadNextScene(SceneConstants.Gameplay);
            }

            else
            {
                await CloseAsync();
                await BuyLivesPopup.CreateFromAddress(CommonPopupPaths.LivesPopupPath);
            }
        }

        private async UniTask CloseAsync()
        {
            animator.SetTrigger(_closeHash);
            background.ShowBackground(false);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
            base.Close();
        }

        private void InitLevelTarget(LevelModel levelModel)
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

        protected override void DoDisappear()
        {
            MainhomeManager.Instance?.SetInputActive(true);

            if (!IsPreload)
                AudioManager.Instance.PlaySoundEffect(SoundEffectType.PopupClose);
        }
    }
}
