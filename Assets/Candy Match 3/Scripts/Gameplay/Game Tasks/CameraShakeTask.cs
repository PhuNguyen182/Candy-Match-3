using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using CandyMatch3.Scripts.Common.Messages;
using Random = UnityEngine.Random;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class CameraShakeTask : IDisposable
    {
        private readonly CinemachineImpulseSource _cinemachineImpulseSource;
        private readonly ISubscriber<CameraShakeMessage> _cameraShakeSubscriber;
        private IDisposable _disposable;

        public CameraShakeTask(CinemachineImpulseSource cinemachineImpulseSource)
        {
            _cinemachineImpulseSource = cinemachineImpulseSource;
            var builder = DisposableBag.CreateBuilder();
            _cameraShakeSubscriber = GlobalMessagePipe.GetSubscriber<CameraShakeMessage>();
            _cameraShakeSubscriber.Subscribe(ShakeCamera).AddTo(builder);
            _disposable = builder.Build();
        }

        private void ShakeCamera(CameraShakeMessage message)
        {
            _cinemachineImpulseSource.m_ImpulseDefinition = new()
            {
                m_ImpulseDuration = message.Duration,
                m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            };

            Vector3 velocity = Random.insideUnitCircle.normalized;
            _cinemachineImpulseSource.GenerateImpulse(velocity * message.Amplitude);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
