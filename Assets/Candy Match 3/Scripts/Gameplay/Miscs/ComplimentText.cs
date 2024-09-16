using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CandyMatch3.Scripts.Common.Enums;
using System.Linq;

namespace CandyMatch3.Scripts.Gameplay.Miscs
{
    public class ComplimentText : MonoBehaviour
    {
        [Serializable]
        public struct Compliment
        {
            public ComplimentEnum ComplimentType;
            public Sprite ComplimentSprite;
        }

        [SerializeField] private Image complimentImage;
        [SerializeField] private List<Compliment> compliments;

        private Dictionary<ComplimentEnum, Sprite> _complimentCollection;

        private void Awake()
        {
            _complimentCollection = compliments.ToDictionary(key => key.ComplimentType, value => value.ComplimentSprite);
        }

        public void ShowCompliment(ComplimentEnum compliment)
        {
            Sprite sprite = _complimentCollection[compliment];
            complimentImage.sprite = sprite;
        }
    }
}
