using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;

namespace CandyMatch3.Scripts.Gameplay.Models.Match
{
    public abstract class BaseMatchModel
    {
        /// <summary>
        /// This property can be rotated, so do not list all cases in this collection
        /// </summary>
        protected abstract List<Vector3Int> matchCellPositions { get; }

        public abstract List<IGridCell> GetMatchResult(Vector3Int gridPosition, Vector3Int inDirection);

        public bool CheckMatch()
        {
            // To do: Do match check logic here
            return false;
        }

        public List<Vector3Int> GetRotatePositions(List<Vector3Int> checkPosition, int angle)
        {
            int count = checkPosition.Count;
            List<Vector3Int> rotateMatchPositions = new();

            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Matrix4x4 rotateMatrix = Matrix4x4.Rotate(rotation);

            for (int i = 0; i < checkPosition.Count; i++)
            {
                Vector3 rotatePosition = rotateMatrix.MultiplyPoint3x4(checkPosition[i]);
                
                int x = Mathf.RoundToInt(rotatePosition.x);
                int y = Mathf.RoundToInt(rotatePosition.y);
                
                Vector3Int newPosition = new(x, y);
                rotateMatchPositions.Add(newPosition);
            }

            return rotateMatchPositions;
        }
    }
}
