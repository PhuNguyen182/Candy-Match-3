using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public class SequencePosition
    {
        public List<Vector3Int> Sequence { get; }

        public SequencePosition(List<Vector3Int> sequence)
        {
            Sequence = sequence;
        }
    }
}
