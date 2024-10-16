using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.InGameBooster
{
    public class InGameBoosterPanel : MonoBehaviour
    {
        [Header("Booster Buttons")]
        [SerializeField] private BoosterButton breakBooster;
        [SerializeField] private BoosterButton blastBooster;
        [SerializeField] private BoosterButton swapBooster;
        [SerializeField] private BoosterButton colorfulBooster;

        [Space(10)]
        [SerializeField] private Transform settingButtonContainer;
        [SerializeField] private InGameBoosterMessage inGameBoosterMessage;

        [Space(10)]
        [SerializeField] private SortingGroup levelSortingGroup;
        [SerializeField] private SortingGroup effectSortingGroup;

        private const string BreakBoosterMessage = "Choose an item to break it";
        private const string BlastBoosterMessage = "Select a cell to blast there";
        private const string SwapBoosterMessage = "Swap any item";
        private const string ColorfulBoosterMessage = "Remove all items that have same color you choose";

        private List<BoosterButton> _boosterButtons;
        public Transform SettingButtonContainer => settingButtonContainer;

        public List<BoosterButton> BoosterButtons
        {
            get
            {
                if(_boosterButtons == null)
                {
                    _boosterButtons = new()
                    {
                        breakBooster,
                        blastBooster,
                        swapBooster,
                        colorfulBooster
                    };
                }

                return _boosterButtons;
            }
        }

        public BoosterButton GetBoosterButtonByType(InGameBoosterType boosterType)
        {
            return BoosterButtons.FirstOrDefault(booster => booster.BoosterType == boosterType);
        }

        public void ShowBoosterMessage(InGameBoosterType boosterType)
        {
            string message = boosterType switch
            {
                InGameBoosterType.Break => BreakBoosterMessage,
                InGameBoosterType.Blast => BlastBoosterMessage,
                InGameBoosterType.Swap => SwapBoosterMessage,
                InGameBoosterType.Colorful => ColorfulBoosterMessage,
                _ => ""
            };

            inGameBoosterMessage.SetMessage(message);
        }

        public async UniTask SetBoosterPanelActive(bool isActive)
        {
            await inGameBoosterMessage.SetMessageActive(isActive);
            levelSortingGroup.enabled = isActive;
            effectSortingGroup.enabled = isActive;
        }
    }
}
