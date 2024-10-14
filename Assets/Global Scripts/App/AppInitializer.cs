using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Service;
using CandyMatch3.Scripts.GameData;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace GlobalScripts.App
{
    public class AppInitializer : Singleton<AppInitializer>, IService
    {
        protected override void OnAwake()
        {
            Initialize();
        }

        public void Initialize()
        {
            LoadGameData();
            InitDOTween();
            InitUnitask();
        }

        private void LoadGameData()
        {
            GameDataManager.Instance.LoadData();
            GameDataManager.Instance.InitializeData();
        }

        private void InitDOTween()
        {
            DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(5000, 1000);
        }

        private void InitUnitask()
        {
            TaskPool.SetMaxPoolSize(10000);
        }
    }
}
