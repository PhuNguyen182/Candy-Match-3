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
            impulseSource.GenerateImpulseWithVelocity(Random.insideUnitCircle.normalized * m);
        }
    }
}
