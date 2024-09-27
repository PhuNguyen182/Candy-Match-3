using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Constants
{
    public struct Match3Constants
    {
        public const int MatchDelayFrame = 3;
        public const int BoosterDelayFrame = 3;
        public const float ExplosionDelay = 1.667f;

        public const float ExplosionPower = 0.0001f;
        public const float ExplodeAmplitude = 1.75f;
        public const float ItemMatchDelay = 0.166f;
        public const float RegionMatchDelay = 0.167f;

        public const float BaseItemMoveSpeed = 9f;
        public const float FallenAccelaration = 1.618f;
        public const float TouchMoveTorerance = 0.25f; // 0.5f * 0.5f

        public const float NearSpeed = 1f;
        public const float FarSpeed = 18f;
        public const float MaxDistance = 12f;

        public const float ComboStripedWrappedDelay = 0.25f;
        public const float ComboDoubleWrappedDelay = 0.666f;

        public static List<Vector3Int> AdjacentSteps = new() { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
    }
}
