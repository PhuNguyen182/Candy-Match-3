using System;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    [Serializable]
    public struct ExplodeEffectData
    {
        public float Timer;
        public float Scale;

        [Header("Explode Config")]
        public float DistortionStrength;
        public float PropagationSpeed;
        public float Wave;
        public float Magnitude;
    }
}
