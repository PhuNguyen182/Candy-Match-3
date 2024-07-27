using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IBlockItem
    {
        public int ItemID { get; }
        public bool IsMatchable { get; }
        
        public ItemType ItemType { get; }
        public CandyColor CandyColor { get; }

        public Vector3 WorldPosition { get; }
        public Vector3Int GridPosition { get; set; }

        public UniTask ItemBlast();

        public void SetWorldPosition(Vector3 position);
        public void ReleaseItem();
    }
}
