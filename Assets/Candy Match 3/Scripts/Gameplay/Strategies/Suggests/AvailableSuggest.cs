using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Strategies.Suggests
{
    public struct AvailableSuggest
    {
        public int Score;
        public bool IsSwapWithBooster;
        public List<Vector3Int> Positions;

        public Vector3Int Position;
        public Vector3Int Direction;

        public override string ToString()
        {
            StringBuilder builder = new();
            builder.Append($"From: {Position} -> To: {Position + Direction},  ");
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
