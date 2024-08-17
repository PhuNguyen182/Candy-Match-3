using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Models;

namespace CandyMatch3.Scripts.Common.SingleConfigs
{
    public class PlayGameConfig : BaseSingleConfig<PlayGameConfig>
    {
        public bool IsTestMode;
        public LevelModel LevelModel;
    }
}
