using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Miscs
{
    public class MultiSpriteButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite[] states;

        public void AddListener(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void SetState(int index)
        {
            Sprite state = states[index % states.Length];
            buttonImage.sprite = state;
        }
    }
}
