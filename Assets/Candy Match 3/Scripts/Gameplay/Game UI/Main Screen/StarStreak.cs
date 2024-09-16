using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class StarStreak : MonoBehaviour
    {
        [SerializeField] private GameObject star;
        [SerializeField] private RectTransform rectTransform;

        public void SetStarActive(bool active)
        {
            star.SetActive(active);
        }

        public void SetPosition(float x)
        {
            float y = rectTransform.anchoredPosition.y;
            rectTransform.anchoredPosition = new(x, y);
        }
    }
}
