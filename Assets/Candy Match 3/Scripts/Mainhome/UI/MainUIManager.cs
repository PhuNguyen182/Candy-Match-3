using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Mainhome.UI.ResourcesDisplayer;

namespace CandyMatch3.Scripts.Mainhome.UI
{
    public class MainUIManager : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private CoinCounter coinCounter;
        [SerializeField] private LifeCounter lifeCounter;

        private void Awake()
        {
            backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnBackClicked()
        {

        }
    }
}
