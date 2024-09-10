using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameTasks.BoosterTasks 
{
    public class BreakBoosterTask
    {
        private readonly BreakGridTask _breakGridTask;

        public BreakBoosterTask(BreakGridTask breakGridTask)
        {
            _breakGridTask = breakGridTask;
        }

        public async UniTask Activate(Vector3Int position)
        {
            await _breakGridTask.Break(position);
        }
    } 
}
