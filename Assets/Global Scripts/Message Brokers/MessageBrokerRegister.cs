using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessagePipe;

namespace GlobalScripts.MessageBrokers
{
    public class MessageBrokerRegister
    {
        private IServiceProvider _provider;
        private BuiltinContainerBuilder _builder;

        public void InitializeMessages()
        {
            _builder = new();
            _builder.AddMessagePipe();

            AddMeggageBrokers();
            _provider = _builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(_provider);
        }

        private void AddMeggageBrokers()
        {
            //_builder.AddMessageBroker<PowerupMessage>();
        }
    }
}
