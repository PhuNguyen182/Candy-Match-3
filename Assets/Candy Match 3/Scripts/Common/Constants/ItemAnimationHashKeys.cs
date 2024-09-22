using UnityEngine;

namespace CandyMatch3.Scripts.Common.Constants
{
    public struct ItemAnimationHashKeys
    {
        public static readonly int BounceHash = Animator.StringToHash("Bounce");
        public static readonly int SuggestHash = Animator.StringToHash("IsSuggest");
        public static readonly int ExplodeHash = Animator.StringToHash("Explode");

        public static readonly int JumpDownHash = Animator.StringToHash("JumpDown");
        public static readonly int AmptitudeHash = Animator.StringToHash("Amptitude");
        public static readonly int FallDownHash = Animator.StringToHash("FallDown");
        public static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        public static readonly int StartHash = Animator.StringToHash("Start");
        public static readonly int KillHash = Animator.StringToHash("Kill");
        public static readonly int MatchHash = Animator.StringToHash("Match");
 
        public static readonly int ComboStripedWrappedHash = Animator.StringToHash("ComboStripedWrapped");
        public static readonly int ComboDoubleWrappedHash = Animator.StringToHash("ComboDoubleWrapped");
        
        public static readonly int IsFirstHash = Animator.StringToHash("IsFirst");
        public static readonly int DirectionHash = Animator.StringToHash("Direction");
    }
}
