using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class StarStreak : MonoBehaviour
    {
        [SerializeField] private GameObject star;
        [SerializeField] private RectTransform rectTransform;

        public void SetStarActive(bool active)
        {
            // Check if the star is activated before
            bool hasActive = star.activeInHierarchy;

            star.SetActive(active);
            if (active && !hasActive)
                EffectManager.Instance.PlaySoundEffect(SoundEffectType.StarProgressBar);
        }

        public void SetPosition(float x)
        {
            float y = rectTransform.anchoredPosition.y;
            rectTransform.anchoredPosition = new(x, y);
        }
    }
}
