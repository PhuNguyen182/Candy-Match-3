using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Gameplay.GameUI.MainScreen;
using CandyMatch3.Scripts.Common.DataStructs;
using TMPro;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class LoseGamePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] private TargetElement targetElement;
        [SerializeField] private Transform targetContainer;

        private readonly int _closeHash = Animator.StringToHash("Close");

        private CancellationToken _token;
        private UniTaskCompletionSource _source;

        private List<TargetElement> _remainTargets = new();
        private GameObject _background;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
            continueButton.onClick.AddListener(() => OnContinueClicked().Forget());
        }

        public void SetBackground(GameObject background)
        {
            _background = background;
        }

        public UniTask ShowLosePanel()
        {
            _source = new();
            gameObject.SetActive(true);
            return _source.Task;
        }

        public void UpdateScore(int score)
        {
            scoreText.text = $"Your score: <color=#B83555>{score:N1, ru-RU}</color>";
        }

        public void ShowRemainTarget(List<TargetElement> remainTargets)
        {
            ClearRemainTargets();

            for (int i = 0; i < remainTargets.Count; i++)
            {
                TargetView targetView = remainTargets[i].GetView();
                TargetStats targetStats = remainTargets[i].GetStats();
                targetStats.IsFailed = false; // Do not show cross icon in lose panel

                TargetElement target = SimplePool.Spawn(targetElement, targetContainer
                                        , targetContainer.position, Quaternion.identity);
                target.transform.localScale = Vector3.one;
                
                target.UpdateTargetView(targetView);
                target.UpdateTargetCount(targetStats);
                _remainTargets.Add(target);
            }
        }

        private void ClearRemainTargets()
        {
            if(_remainTargets.Count > 0)
            {
                for (int i = 0; i < _remainTargets.Count; i++)
                    SimplePool.Despawn(_remainTargets[i].gameObject);

                _remainTargets.Clear();
            }
        }

        private async UniTask OnContinueClicked()
        {
            await Close();
            _source.TrySetResult();
            gameObject.SetActive(false);
        }

        private async UniTask Close()
        {
            popupAnimator.SetTrigger(_closeHash);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: _token);
        }
    }
}