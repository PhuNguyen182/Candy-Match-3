using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.GameUI.Miscs;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class EndGameScreen : MonoBehaviour
    {
        [SerializeField] private FadeBackground background;
        [SerializeField] private SpecialPanel specialPanel;
        [SerializeField] private WinGamePanel winGamePanel;
        [SerializeField] private LoseGamePanel loseGamePanel;
        [SerializeField] private ContinuePopup continuePopup;

        public void WinSetScoreAndStars(int score, int stars)
        {
            winGamePanel.SetScoreAndStars(score, stars);
        }

        public void SetLoseScore(int score)
        {
            loseGamePanel.UpdateScore(score);
        }

        public void ShowBackground(bool isActive)
        {
            background.ShowBackground(isActive);
        }

        public UniTask ShowWinGame()
        {
            return winGamePanel.ShowWinGame();
        }

        public UniTask ShowLoseGame(List<TargetElement> remainTargets)
        {
            loseGamePanel.ShowRemainTarget(remainTargets);
            return loseGamePanel.ShowLosePanel();
        }

        public UniTask<bool> ShowContinue()
        {
            return continuePopup.ShowContinue();
        }
    }
}
