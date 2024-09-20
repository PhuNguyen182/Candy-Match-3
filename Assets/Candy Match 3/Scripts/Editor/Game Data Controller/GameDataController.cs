using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CandyMatch3.Scripts.GameData;

namespace CandyMatch3.Scripts.Editor.GameDataControl
{
    public class GameDataController : EditorWindow
    {
        [MenuItem("Game Data/Clear Data")]
        public static void ClearData()
        {
            GameDataManager.Instance.DeleteData();
        }
    }
}
