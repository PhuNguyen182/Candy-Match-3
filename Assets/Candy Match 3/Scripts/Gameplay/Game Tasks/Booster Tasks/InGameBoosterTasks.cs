using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Strategies;
using CandyMatch3.Scripts.Gameplay.GridCells;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks
{
    public class InGameBoosterTasks : IDisposable
    {
        private readonly SwapItemTask _swapItemTask;
        private readonly GridCellManager _gridCellManager;
        private readonly ItemManager _itemManager;

        public async UniTask ActivateBooster(InGameBoosterType boosterType)
        {
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}
