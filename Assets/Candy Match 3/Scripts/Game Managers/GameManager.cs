using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.GameData;

namespace CandyMatch3.Scripts.GameManagers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private HeartTimeManager heartManager;

        private bool _isDeleted = false;

        public HeartTimeManager HeartTimer => heartManager;

        private void Start()
        {
            heartManager.LoadHeartOnStart();
        }

        private void Update()
        {
            if (heartManager != null)
                heartManager.UpdateHeartTime();
        }

        private void OnDestroy()
        {
            if (!_isDeleted)
                GameDataManager.Instance.SaveData();
        }

#if !UNITY_EDITOR
        private void OnApplicationQuit()
        {
            GameDataManager.Instance.SaveData();
        }
#endif

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        private void OnApplicationFocus(bool focus)
        {
            if(focus)
                GameDataManager.Instance.SaveData();
        }

        private void OnApplicationPause(bool pause)
        {
            if(pause)
                GameDataManager.Instance.SaveData();
        }
#endif
    }
}
