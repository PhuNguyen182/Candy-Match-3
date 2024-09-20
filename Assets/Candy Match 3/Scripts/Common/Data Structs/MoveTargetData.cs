using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.DataStructs
{
    public struct MoveTargetData
    {
        public bool IsCompleted;
        public TargetEnum TargetType;
        public Vector3 Destination;
    }
}
