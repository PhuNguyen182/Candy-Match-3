using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.MessageBrokers;
using DG.Tweening;

namespace GlobalScripts.Service
{
    public class InitializeService : SingletonClass<InitializeService>, IService
    {
        public void Initialize()
        {
            LoadGameData();
            InitMessageBroker();
            InitDOTween();
        }

        private void InitMessageBroker()
        {
            MessageBrokerRegister messageBroker = new();
            messageBroker.InitializeMessages();
        }

        private void LoadGameData()
        {
            
        }

        private void InitDOTween()
        {
            DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(2000, 200);
        }
    }
}