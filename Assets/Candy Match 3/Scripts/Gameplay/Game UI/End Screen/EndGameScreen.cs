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

        private void Awake()
        {
            winGamePanel.SetBackground(background);
            loseGamePanel.SetBackground(background);
        }

        public void ShowWinGame()
        {
            winGamePanel.gameObject.SetActive(true);
        }
    }
}
