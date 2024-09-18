using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using CandyMatch3.Scripts.LevelDesign.LevelBuilder;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Common.SingleConfigs
{
    public class LevelPlayInfo
    {
        public static async UniTask<string> GetLevelData(int level)
        {
            string folder = LevelFolderClassifyer.GetLevelRangeFolderName(level);
            string levelPath = $"Level Data/{folder}/level_{level}.txt";
            
            TextAsset levelText = await Addressables.LoadAssetAsync<TextAsset>(levelPath);
            string levelData = levelText != null ? levelText.text : null;
            
            return levelData;
        }
    }
}
