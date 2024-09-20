using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CandyMatch3.Scripts.Common.UIElements
{
    public class SliderButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Animator animator;

        [Header("Button States")]
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        private readonly int _onHash = Animator.StringToHash("On");
        private readonly int _offHash = Animator.StringToHash("Off");
        private readonly int _enableHash = Animator.StringToHash("Enable");
        private readonly int _disableHash = Animator.StringToHash("Disable");

        public void AddListener(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void UpdateImmediately(bool value)
        {
            animator.SetTrigger(value ? _onHash : _offHash);
            SetSprite(value);
        }

        public void UpdateValue(bool value)
        {
            animator.SetTrigger(value ? _enableHash : _disableHash);
            SetSprite(value);
        }

        private void SetSprite(bool isActive)
        {
            if (background != null)
                background.sprite = isActive ? onSprite : offSprite;
        }
    }
}
