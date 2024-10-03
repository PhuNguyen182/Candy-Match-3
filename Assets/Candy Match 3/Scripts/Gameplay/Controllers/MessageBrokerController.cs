using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Messages;
using CandyMatch3.Scripts.Common.DataStructs;
using GlobalScripts.Utils;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.Controllers
{
    /*
     * Split into multiple message broker regisgers like this to reduce overhead of too much messages
     */

    public class MessageBrokerController
    {
        private readonly IServiceProvider _provider;
        private readonly BuiltinContainerBuilder _builder;

        public MessageBrokerController()
        {
            _builder = new();
            _builder.AddMessagePipe();

            AddMeggageBrokers();
            _provider = _builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(_provider);
        }

        private void AddMeggageBrokers()
        {
            _builder.AddMessageBroker<DecreaseMoveMessage>();
            _builder.AddMessageBroker<DecreaseTargetMessage>();
            _builder.AddMessageBroker<AsyncMessage<MoveTargetData>>();

            _builder.AddMessageBroker<CameraShakeMessage>();
            _builder.AddMessageBroker<AddInGameBoosterMessage>();
            _builder.AddMessageBroker<UseInGameBoosterMessage>();

            _builder.AddMessageBroker<BoardStopMessage>();
            _builder.AddMessageBroker<BreakExpandableMessage>();
            _builder.AddMessageBroker<ExpandMessage>();
            _builder.AddMessageBroker<ComplimentMessage>();

            _builder.AddMessageBroker<UpdateResourceMessage>();
        }
    }
}
