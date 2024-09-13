using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class StarStreak : MonoBehaviour
    {
        [SerializeField] private GameObject star;

        public void SetStarActive(bool active)
        {
            star.SetActive(active);
        }
    }
}
