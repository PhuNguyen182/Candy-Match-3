using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.Popups
{
    public class InGameBoosterMessage : MonoBehaviour
    {
        [SerializeField] private Animator panelAnimator;
        [SerializeField] private TMP_Text messageText;

        private CancellationToken _token;
        private readonly int _closeHash = Animator.StringToHash("Close");

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public void SetMessage(string message)
        {
            messageText.text = message;
        }

        public async UniTask SetMessageActive(bool active)
        {
            if (active)
            {
                gameObject.SetActive(true);
            }

            else
            {
                panelAnimator.SetTrigger(_closeHash);
                await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
                gameObject.SetActive(false);
            }
        }
    }
}
