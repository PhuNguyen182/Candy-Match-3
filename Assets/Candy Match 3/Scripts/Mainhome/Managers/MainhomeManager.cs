using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Mainhome.Inputs;
using CandyMatch3.Scripts.Mainhome.UI;

namespace CandyMatch3.Scripts.Mainhome.Managers
{
    public class MainhomeManager : MonoBehaviour
    {
        [SerializeField] private MainhomeInput mainhomeInput;
        [SerializeField] private MainUIManager mainUIManager;

        private MainhomeMessageManager _mainhomeMessageManager;

        public static MainhomeManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            _mainhomeMessageManager = new();
            mainhomeInput.IsActived = true;
        }

        public void SetInputActive(bool isActive)
        {
            mainhomeInput.IsActived = isActive;
        }

        private void OnDestroy()
        {
            GC.Collect();
        }
    }
}
