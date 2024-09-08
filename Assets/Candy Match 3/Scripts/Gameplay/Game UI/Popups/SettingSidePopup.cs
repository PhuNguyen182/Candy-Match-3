using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class SettingSidePopup : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button musicButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Animator animator;

        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            quitButton.onClick.AddListener(Quit);
            musicButton.onClick.AddListener(MusicButton);
            soundButton.onClick.AddListener(SoundButton);
        }

        private void Quit()
        {

        }

        private void MusicButton()
        {

        }

        private void SoundButton()
        {

        }
    }
}
