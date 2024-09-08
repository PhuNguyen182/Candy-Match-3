using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.GameUI.EndScreen
{
    public class LoseGamePanel : MonoBehaviour
    {
        private UniTaskCompletionSource<bool> _completionSource;

        private GameObject _background;

        public void SetBackground(GameObject background)
        {
            _background = background;
        }

        public UniTask<bool> ShowLoseGame()
        {
            _completionSource = new();
            _completionSource.TrySetResult(false);
            return _completionSource.Task;
        }
    }
}
