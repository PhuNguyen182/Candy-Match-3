using System;
using System.Collections;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.GameData;
using CandyMatch3.Scripts.GameData.Constants;
using UnityEngine;

namespace CandyMatch3.Scripts.GameManagers
{
    public class HeartTimeManager : MonoBehaviour
    {
        private DateTime _savedHeartTime;
        private TimeSpan _heartTimeDiff;
        private TimeSpan _offset;

        private readonly int _maxHeart = GameDataConstants.MaxLives;
        private readonly int _heartCooldown = GameDataConstants.NextLifeTimer;

        public TimeSpan HeartTimeDiff => _heartTimeDiff;

        public void UpdateHeartTime()
        {
            if (GameDataManager.Instance.GetResource(GameResourceType.Life) < _maxHeart)
            {
                _savedHeartTime = GameDataManager.Instance.GetCurrentHeartTime();
                _offset = DateTime.Now.Subtract(_savedHeartTime);
                _heartTimeDiff = TimeSpan.FromSeconds(_heartCooldown).Subtract(_offset);

                if (_heartTimeDiff.TotalSeconds <= 0)
                {
                    GameDataManager.Instance.EarnResource(GameResourceType.Life, 1);
                    if (GameDataManager.Instance.GetResource(GameResourceType.Life) >= _maxHeart)
                    {
                        _savedHeartTime = DateTime.Now;
                        GameDataManager.Instance.SetResource(GameResourceType.Life, _maxHeart);
                        GameDataManager.Instance.SaveHeartTime(_savedHeartTime);
                    }

                    else
                    {
                        _savedHeartTime = _savedHeartTime.AddSeconds(_heartCooldown);
                        GameDataManager.Instance.SaveHeartTime(_savedHeartTime);
                    }
                }
            }

            else GameDataManager.Instance.SetResource(GameResourceType.Life, _maxHeart);
        }

        public void LoadHeartOnStart()
        {
            _savedHeartTime = GameDataManager.Instance.GetCurrentHeartTime();
            TimeSpan diff = DateTime.Now.Subtract(_savedHeartTime);

            do
            {
                TimeSpan cooldown = TimeSpan.FromSeconds(_heartCooldown);
                diff = diff.Subtract(cooldown);

                if(diff.TotalSeconds > 0)
                    GameDataManager.Instance.EarnResource(GameResourceType.Life, 1);

                if (GameDataManager.Instance.GetResource(GameResourceType.Life) >= _maxHeart)
                {
                    _savedHeartTime = DateTime.Now;
                    GameDataManager.Instance.SetResource(GameResourceType.Life, _maxHeart);
                    GameDataManager.Instance.SaveHeartTime(DateTime.Now);
                    break;
                }

                else
                    _savedHeartTime = _savedHeartTime.Add(TimeSpan.FromSeconds(_heartCooldown));
            } while (diff.TotalSeconds > 0);
        }
    }
}
