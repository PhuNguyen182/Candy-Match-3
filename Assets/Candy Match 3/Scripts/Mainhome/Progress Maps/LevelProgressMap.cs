using R3;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using CandyMatch3.Scripts.Mainhome.UI.Popups;
using CandyMatch3.Scripts.Gameplay.GameUI.Popups;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Common.Constants;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.Mainhome.ProgressMaps
{
    public class LevelProgressMap : MonoBehaviour
    {
        [SerializeField] private int minLevel = 1;
        [SerializeField] private int maxLevel = 100;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private Camera mainCamera;

        [Header("Progress Map")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private PathFollower pathFollower;
        [SerializeField] private List<LevelNodeButton> nodeButtons;

        private Dictionary<int, LevelNodeButton> _nodePathDict;
        private IDisposable _disposable;

        public int MinLevel => minLevel;
        public int MaxLevel => maxLevel;

        private void Awake()
        {
            PreloadPopups();
            InitProgressLevel();
        }

        private void PreloadPopups()
        {
            AlertPopup.PreloadFromAddress(CommonPopupPaths.AlertPopupPath).Forget();
            StartGamePopup.PreloadFromAddress(CommonPopupPaths.StartGamePopupPath).Forget();
        }

        [Button]
        public void FetchBodePaths()
        {
            nodeButtons.Clear();

            for (int i = 0; i < nodeContainer.childCount; i++)
            {
                if (nodeContainer.GetChild(i).TryGetComponent<LevelNodeButton>(out var node))
                {
                    node.SetCanvasCamera(mainCamera);
                    node.gameObject.name = $"Level {node.Level} Node";
                    nodeButtons.Add(node);
                }
            }
        }

        public LevelNodeButton GetLevelNode(int level)
        {
            return _nodePathDict[level];
        }

        public async UniTask Move(int startIndex, int endIndex)
        {
            await pathFollower.Move(startIndex, endIndex, moveDuration);
        }

        public void Translate(int level)
        {
            pathFollower.Translate(level);
        }

        private void InitProgressLevel()
        {
            //int currentLevel = GameData.Instance.GetCurrentLevel();
            using (var listpool = ListPool<IDisposable>.Get(out var disposables))
            {
                _nodePathDict = nodeButtons.ToDictionary(node => node.Level, node =>
                {
                    //node.SetAvailableState(currentLevel >= node.Level);
                    //bool isLevelComplete = GameData.Instance.IsLevelComplete(node.Level);

                    // Check less than node.Level to ensure all completed level are in idle state without animation
                    //if (isLevelComplete && currentLevel >= node.Level)
                    //{
                    //    var levelNode = GameData.Instance.GetLevelProgress(node.Level);
                    //    node.SetIdleState(levelNode.Star, false);
                    //}

                    IDisposable d = node.OnClickObservable.Select(value => (value.Level, value.Star))
                                        .Subscribe(value => OnNodeButtonClick(value.Level, value.Star).Forget());
                    disposables.Add(d);
                    return node;
                });

                _disposable = Disposable.Combine(disposables.ToArray());
            }
        }

        private async UniTask OnNodeButtonClick(int level, int stars)
        {
            //int currentLevel = GameData.CurrentLevel; // Fake code
            //if (level > currentLevel)
            //{
            //    var alertPopup = await AlertPopup.CreateFromAddress(AlertPopupPath);
            //    alertPopup.SetMessage("This level is still locked.");
            //}

            //else
            {
                var startGamePopup = await StartGamePopup.CreateFromAddress(CommonPopupPaths.StartGamePopupPath);
                await startGamePopup.SetLevelInfo(new LevelBoxData
                {
                    Level = level,
                    Stars = stars
                });
            }
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}
