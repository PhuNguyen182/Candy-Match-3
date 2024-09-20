using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Common.Messages
{
    public struct ActivateBoosterMessage
    {
        public Vector3Int Position;
        public IBlockItem Sender;
    }
}
