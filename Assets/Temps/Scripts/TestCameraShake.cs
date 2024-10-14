using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TestCameraShake : MonoBehaviour
{
    public float m;
    [Range(-1f, 1f)]
    public float x;
    [Range(-1f, 1f)]
    public float y;
    public float result;
    public CinemachineImpulseSource impulseSource;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    impulseSource.m_ImpulseDefinition = new CinemachineImpulseDefinition
        //    {
        //        m_ImpulseDuration = 0.3f,
        //        m_ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Explosion,
        //        m_ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
        //    };

        //    impulseSource.GenerateImpulseWithVelocity(Random.insideUnitCircle.normalized * m);
        //}

        result = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }
}
