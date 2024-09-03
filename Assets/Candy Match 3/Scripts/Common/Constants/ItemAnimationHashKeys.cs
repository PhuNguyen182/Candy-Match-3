using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Constants
{
    public struct ItemAnimationHashKeys
    {
        public static readonly int BounceHash = Animator.StringToHash("Bounce");
        public static readonly int JumpDownHash = Animator.StringToHash("JumpDown");
        public static readonly int AmptitudeHash = Animator.StringToHash("Amptitude");
        public static readonly int MatchHash = Animator.StringToHash("Match");
        public static readonly int KillHash = Animator.StringToHash("Kill");
        public static readonly int StartHash = Animator.StringToHash("Start");
    }
}
