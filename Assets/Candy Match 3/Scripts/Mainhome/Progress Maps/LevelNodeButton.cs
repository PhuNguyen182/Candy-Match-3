using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyMatch3.Scripts.Mainhome.ProgressMaps
{
    public class LevelNodeButton : MonoBehaviour
    {
        [SerializeField] private int level;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Button levelButton;
        [SerializeField] private LevelNodeStar levelNodeStar;
        [SerializeField] private GameObject highlightRing;
        [SerializeField] private Canvas buttonCanvas;

        [Header("Level Texts")]
        [SerializeField] private TMP_Text blueLevelText;
        [SerializeField] private TMP_Text pinkLevelText;

        [Header("Button Sprites")]
        [SerializeField] private Sprite disableState;
        [SerializeField] private Sprite enableBlueState;
        [SerializeField] private Sprite enablePinkState;

        private int _levelStars;
        private bool _isAvailable;

        public int Level => level;

        public Observable<(int Level, int Star)> OnClickObservable
            => levelButton.OnClickAsObservable()
                          .Select(_ => (level, _levelStars));

        private void OnEnable()
        {
            // To do: highlightRing.SetActive("If current level is equal to this level node")
        }

        public void SetCanvasCamera(Camera camera)
        {
            buttonCanvas.worldCamera = camera;
        }

        public void CheckCurrent(bool isCurrent)
        {
            if (isCurrent)
            {
                buttonImage.sprite = enableBlueState;
                pinkLevelText.gameObject.SetActive(false);
            }

            highlightRing.SetActive(isCurrent);
            blueLevelText.gameObject.SetActive(isCurrent);
        }

        public void SetAvailable(bool isAvailable)
        {
            _isAvailable = isAvailable;
            levelNodeStar.gameObject.SetActive(_isAvailable);
            pinkLevelText.gameObject.SetActive(_isAvailable);

            if (!_isAvailable)
            {
                buttonImage.sprite = disableState;
            }
        }

        public void SetStarState(int star, bool isRecentWin)
        {
            levelNodeStar.UpdateStars(star, isRecentWin);
            buttonImage.sprite = enablePinkState;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetLevelText();
        }
#endif

        private void SetLevelText()
        {
            blueLevelText.text = $"{level}";
            pinkLevelText.text = $"{level}";
        }
    }
}
