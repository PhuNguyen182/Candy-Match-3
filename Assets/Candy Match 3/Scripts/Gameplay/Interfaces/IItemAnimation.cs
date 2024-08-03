using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemAnimation
    {
        public UniTask BounceOnTap();
        public UniTask BounceInDirection(Vector3 direction);
        public UniTask MoveTo(Vector3 position);
        public UniTask JumpDown(float amplitude);
    }
}
