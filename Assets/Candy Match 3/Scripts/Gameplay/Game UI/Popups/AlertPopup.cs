using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class AlertPopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Animator popupAnimator;

        private readonly int _closeHash = Animator.StringToHash("CloseAnimation");

        private void Awake()
        {
            
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }
    }
}
