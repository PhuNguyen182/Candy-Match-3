using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class QuitPopup : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Animator popupAnimator;

        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            continueButton.onClick.AddListener(Continue);
            quitButton.onClick.AddListener(Quit);
            closeButton.onClick.AddListener(Quit);
        }

        private void Continue()
        {

        }

        private void Quit()
        {

        }

        private async UniTask Close()
        {
            await UniTask.CompletedTask;
        }
    }
}
