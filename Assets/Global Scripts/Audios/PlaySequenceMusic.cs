using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Extensions;
using Cysharp.Threading.Tasks;

namespace GlobalScripts.Audios
{
    public class PlaySequenceMusic : MonoBehaviour
    {
        [SerializeField] private bool shuffle;
        [SerializeField] private float delayStart = 1.5f;
        [SerializeField] private float trackInterval = 0.5f;
        [SerializeField] private List<AudioClip> musicCollection;

        private CancellationToken _token;

        private void Awake()
        {
            _token = this.GetCancellationTokenOnDestroy();
        }

        public void PlayMusicSequence()
        {
            PlaySequence().Forget();
        }

        private async UniTask PlaySequence()
        {
            int index = 0;
            await UniTask.Delay(TimeSpan.FromSeconds(delayStart), cancellationToken: _token);

            if (shuffle)
                musicCollection.Shuffle();

            while (true)
            {
                int musicCount = musicCollection.Count;

                AudioClip music = musicCollection[index % musicCount];
                float duration = music.length;
                index = index + 1;

                await UniTask.Delay(TimeSpan.FromSeconds(trackInterval), cancellationToken: _token);
                AudioManager.Instance.PlayMusic(music, false, 0.65f);
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: _token);
            }
        }
    }
}
