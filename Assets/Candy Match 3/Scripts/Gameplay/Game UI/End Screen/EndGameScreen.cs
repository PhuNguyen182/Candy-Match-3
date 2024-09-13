using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class EndGameScreen : MonoBehaviour
    {
        [SerializeField] private GameObject background;
        [SerializeField] private WinGamePanel winGamePanel;
        [SerializeField] private LoseGamePanel loseGamePanel;
        [SerializeField] private ContinuePopup continuePopup;

        private void Awake()
        {
            winGamePanel.SetBackground(background);
            loseGamePanel.SetBackground(background);
        }

        public UniTask ShowWinGame()
        {
            return winGamePanel.ShowWinGame();
        }

        public UniTask ShowLoseGame()
        {
            return loseGamePanel.ShowLosePanel();
        }

        public UniTask<bool> ShowContinue()
        {
            return continuePopup.ShowContinue();
        }
    }
}
