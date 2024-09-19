using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.GameData.GameResources;
using CandyMatch3.Scripts.GameData.LevelProgresses;
using GlobalScripts.SaveSystem;

namespace CandyMatch3.Scripts.GameData
{
    public class GameDataManager : SingletonClass<GameDataManager>
    {
        private LevelProgressData _levelProgressData;
        private GameResourceManager _gameResourceManager;

        private const string GameResourceDataKey = "GameResources";
        private const string LevelProgressDataKey = "LevelProgressData";

        public void InitializeData()
        {
            _gameResourceManager.Initialize();
        }

        public void LoadData()
        {
            _levelProgressData = SimpleSaveSystem<LevelProgressData>.LoadData(LevelProgressDataKey) ?? new();
            _gameResourceManager = SimpleSaveSystem<GameResourceManager>.LoadData(GameResourceDataKey) ?? new();
        }

        private void ReleaseBackData()
        {
            _gameResourceManager.ReleaseBack();
        }

        public void SaveData()
        {
            ReleaseBackData();

            SimpleSaveSystem<LevelProgressData>.SaveData(LevelProgressDataKey, _levelProgressData);
            SimpleSaveSystem<GameResourceManager>.SaveData(GameResourceDataKey, _gameResourceManager);
        }

        public void DeleteData()
        {
            SimpleSaveSystem<GameResourceManager>.DeleteData(GameResourceDataKey);
            SimpleSaveSystem<LevelProgressData>.DeleteData(LevelProgressDataKey);
        }

        public int GetResource(GameResourceType resourceType)
        {
            return _gameResourceManager.GetResource(resourceType);
        }

        public void SetResource(GameResourceType resourceType, int amount)
        {
            _gameResourceManager.SetResource(resourceType, amount);
        }

        public void EarnResource(GameResourceType resourceType, int amount)
        {
            _gameResourceManager.EarnResource(resourceType, amount);
        }

        public void SpendResource(GameResourceType resourceType, int amount)
        {
            _gameResourceManager.SpendResource(resourceType, amount);
        }

        public void SetLevel(int level)
        {
            _levelProgressData.SetLevel(level);
        }

        public void AddLevelProgress(LevelProgressNode level)
        {
            _levelProgressData.Append(level);
        }

        public bool IsLevelComplete(int level)
        {
            return _levelProgressData.IsLevelComplete(level);
        }

        public LevelProgressNode GetLevelProgress(int level)
        {
            return _levelProgressData.GetLevelProgress(level).DataValue;
        }

        public DateTime GetCurrentHeartTime()
        {
            return _gameResourceManager.GetHeartTime();
        }

        public void SaveHeartTime(DateTime time)
        {
            _gameResourceManager.SetHeartTime(time);
        }
    }
}
