using System;
using System.Collections;
using System.Collections.Generic;
using CandyMatch3.Scripts.Common.Messages;
using UnityEngine;
using MessagePipe;

namespace CandyMatch3.Scripts.Mainhome.Managers
{
    public class MainhomeMessageManager
    {
        private readonly IServiceProvider _provider;
        private readonly BuiltinContainerBuilder _builder;

        public MainhomeMessageManager()
        {
            _builder = new();
            _builder.AddMessagePipe();

            AddMeggageBrokers();
            _provider = _builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(_provider);
        }

        private void AddMeggageBrokers()
        {
            _builder.AddMessageBroker<UpdateResourceMessage>();
        }
    }
}
