using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalScripts.Vibrations;

namespace GlobalScripts.Audios
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private float volumeScale = 0.6f;
        [SerializeField] private AudioClip clip;

        private void Awake()
        {
            button.onClick.AddListener(() =>
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                Vibration.Vibrate(80);
#endif
                AudioManager.Instance.PlaySoundEffect(clip, volumeScale);
            });
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            button ??= GetComponent<Button>();
        }
#endif
    }
}
