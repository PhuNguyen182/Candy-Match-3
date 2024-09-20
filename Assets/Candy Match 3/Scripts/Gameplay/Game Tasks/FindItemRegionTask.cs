using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Gameplay.Models.Match;
using CandyMatch3.Scripts.Gameplay.GridCells;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class FindItemRegionTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;

        public FindItemRegionTask(GridCellManager gridCellManager)
        {
            _gridCellManager = gridCellManager;
        }

        private void Test()
        {
            using MatchableRegion x = new();
        }

        public void Dispose()
        {

        }
    }
}
