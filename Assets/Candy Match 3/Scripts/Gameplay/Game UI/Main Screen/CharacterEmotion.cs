using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.GameUI.MainScreen
{
    public class CharacterEmotion : MonoBehaviour
    {
        [SerializeField] private Animator characterAnimator;
        [SerializeField] private ParticleSystem happyEffect;

        private readonly int _happyHash = Animator.StringToHash("Happy");

        public void ShowHappyState()
        {
            happyEffect.Play();
            characterAnimator.SetTrigger(_happyHash);
        }
    }
}
