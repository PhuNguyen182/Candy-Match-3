using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TestCameraShake : MonoBehaviour
{
    public float m;
    public CinemachineImpulseSource impulseSource;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            impulseSource.m_ImpulseDefinition = new CinemachineImpulseDefinition
            {
                m_ImpulseDuration = 0.3f,
                m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
                m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            };

            impulseSource.GenerateImpulseWithVelocity(Random.insideUnitCircle.normalized * m);
        }
    }
}
