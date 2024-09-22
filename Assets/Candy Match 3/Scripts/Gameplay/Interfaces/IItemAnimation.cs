using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemAnimation
    {
        public void BounceOnTap();
        public UniTask BounceInDirection(Vector3 direction);
        public UniTask MoveTo(Vector3 position, float duration);
        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst);
        public void FallDown(bool isFallDown, float stretch);
        public void JumpDown(float amptitude);
    }
}
