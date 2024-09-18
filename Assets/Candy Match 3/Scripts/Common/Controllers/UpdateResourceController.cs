using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Mainhome.Managers;
using MessagePipe;

namespace CandyMatch3.Scripts.Common.Controllers
{
    public class UpdateResourceController : MonoBehaviour
    {
        private IPublisher<UpdateResourceMessage> _updateResourcePublisher;

        public static UpdateResourceController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _updateResourcePublisher = GlobalMessagePipe.GetPublisher<UpdateResourceMessage>();
        }

        public void UpdateResource(UpdateResourceMessage message)
        {
            _updateResourcePublisher?.Publish(message);
        }
    }
}
