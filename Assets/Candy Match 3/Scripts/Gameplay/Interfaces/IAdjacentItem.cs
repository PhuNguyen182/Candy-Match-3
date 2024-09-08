using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IAdjacentItem
    {
        public void Expand(Vector3Int position);
    }
}
