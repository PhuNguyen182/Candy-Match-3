using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.Mainhome.Managers;
using CandyMatch3.Scripts.Common.SingleConfigs;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Mainhome.ProgressMaps
{
    [RequireComponent(typeof(LevelProgressMap))]
    public class ProgressMapTrigger : MonoBehaviour
    {
        [SerializeField] private float moveLevelNodeDelay = 1.75f;
        [SerializeField] private LevelProgressMap levelProgressMap;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        private void Start()
        {
            OnStartMainhome().Forget();
        }

        private async UniTask OnStartMainhome()
        {
            if (BackHomeConfig.Current != null)
            {
                if (BackHomeConfig.Current.EndResult == EndResult.Win)
                {
                    bool levelIncreased = BackHomeConfig.Current.LevelIncreased;
                    
                    if(levelIncreased) await OnBackHome();
                    else ShowCurrentMainhome();
                }

                else ShowCurrentMainhome();
            }

            else ShowCurrentMainhome();
            BackHomeConfig.Current = null;
        }

        private void ShowCurrentMainhome()
        {
            int level = GameDataManager.Instance.GetCurrentLevel();
            LevelNodeButton levelNode = levelProgressMap.GetLevelNode(level);
            MainhomeManager.Instance.CameraScroller.TranslateTo(levelNode.transform.position);

            levelProgressMap.Translate(level - 1);
        }

        private async UniTask OnBackHome()
        {
            int level = BackHomeConfig.Current.Level;
            int star = BackHomeConfig.Current.Stars;

            LevelNodeButton levelNode = levelProgressMap.GetLevelNode(level);
            MainhomeManager.Instance.CameraScroller.TranslateTo(levelNode.transform.position);
            Translate(level - 1);

            MainhomeManager.Instance.SetInputActive(false);
            await UniTask.Delay(TimeSpan.FromSeconds(moveLevelNodeDelay), cancellationToken: _token);
            await Move(level - 1, level);
            MainhomeManager.Instance.SetInputActive(true);
        }

        private async UniTask Move(int startIndex, int endIndex)
        {
            await levelProgressMap.Move(startIndex, endIndex);
        }

        private void Translate(int level)
        {
            levelProgressMap.Translate(level);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            levelProgressMap ??= GetComponent<LevelProgressMap>();
        }
#endif
    }
}
