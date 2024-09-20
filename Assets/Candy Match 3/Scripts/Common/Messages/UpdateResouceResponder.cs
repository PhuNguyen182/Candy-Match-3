using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using MessagePipe;

namespace CandyMatch3.Scripts.Common.Messages
{
    public class UpdateResouceResponder : MonoBehaviour
    {
        [SerializeField] private GameResourceType resourceType;

        private ISubscriber<UpdateResourceMessage> _updateResourceSubscriber;
        private IDisposable _disposable;

        public Action OnUpdate;

        private void Start()
        {
            RegisterMessage();
        }

        private void RegisterMessage()
        {
            var messageBuilder = DisposableBag.CreateBuilder();
            _updateResourceSubscriber = GlobalMessagePipe.GetSubscriber<UpdateResourceMessage>();
            _updateResourceSubscriber.Subscribe(OnMessageReceived).AddTo(messageBuilder);
            _disposable = messageBuilder.Build();
        }

        private void OnMessageReceived(UpdateResourceMessage message)
        {
            if(message.ResouceType == resourceType)
            {
                OnUpdate?.Invoke();
            }
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}
