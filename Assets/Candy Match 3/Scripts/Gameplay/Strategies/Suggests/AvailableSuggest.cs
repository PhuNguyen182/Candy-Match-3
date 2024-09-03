using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Strategies.Suggests
{
    public struct AvailableSuggest
    {
        public int Score;
        public Vector3Int FromPosition;
        public Vector3Int ToPosition;
        public List<Vector3Int> Positions;

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append($"From: {FromPosition} -> To: {ToPosition},  ");
            builder.Append($"Positions: ");
            
            for (int i = 0; i < Positions.Count; i++)
            {
                builder.Append($"{Positions[i]}  ");
            }

            string text = builder.ToString();
            builder.Clear();
            
            return text;
        }
    }
}
