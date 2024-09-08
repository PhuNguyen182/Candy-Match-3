using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class WinGamePanel : MonoBehaviour
    {
        private GameObject _background;

        public void SetBackground(GameObject background)
        {
            _background = background;
        }
    }
}
