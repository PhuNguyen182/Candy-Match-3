using UnityEngine;

namespace CandyMatch3.Scripts.Common.Messages
{
    public struct ExpandMessage
    {
        public bool CanExpand;
    }

    public struct BreakExpandableMessage
    {
        public Vector3Int Position;
    }
}
