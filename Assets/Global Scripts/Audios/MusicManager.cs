using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Common.Enums;

namespace GlobalScripts.Audios
{
    public class MusicManager : Singleton<MusicManager>
    {
        [SerializeField] private bool playSequence;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource itemSource;
        [SerializeField] private SoundEffectDatabase effectDatabase;
        [SerializeField] private MusicDatabase musicDatabase;
        [SerializeField] private PlaySequenceMusic sequenceMusicPlayer;

        public static event Action<float> OnMasterChange;
        public static event Action<float> OnMusicChange;
        public static event Action<float> OnSoundChange;

        private const string MasterVolumeMixer = "MasterVolume";
        private const string MusicVolumeMixer = "MusicVolume";
        private const string SoundVolumeMixer = "SoundVolume";

        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string SoundVolumeKey = "SoundVolume";

        public float MasterVolume
        {
            get => PlayerPrefs.GetFloat(MasterVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, value);
                OnMasterChange.Invoke(value);
            }
        }

        public float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, value);
                OnMusicChange.Invoke(value);
            }
        }

        public float SoundVolume
        {
            get => PlayerPrefs.GetFloat(SoundVolumeKey, 1);
            set
            {
                PlayerPrefs.SetFloat(SoundVolumeKey, value);
                OnSoundChange.Invoke(value);
            }
        }

        protected override void OnAwake()
        {
            effectDatabase.Initialize();
            musicDatabase.Initialize();
        }

        private void Start()
        {
            OnMasterChange += AdjustMasterVolume;
            OnMusicChange += AdjustMusicVolume;
            OnSoundChange += AdjustSoundVolume;

            MasterVolume += 0;
            MusicVolume += 0;
            SoundVolume += 0;

//#if UNITY_EDITOR
            if (playSequence)
                sequenceMusicPlayer.PlayMusicSequence();
//#endif
        }

        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }

        public void PlayMusic(AudioClip musicClip, bool loop = false, float volume = 1f)
        {
            if (musicClip == null)
                return;

            musicSource.Stop();
            musicSource.loop = loop;
            musicSource.clip = musicClip;
            musicSource.volume = volume;
            musicSource.Play();
        }

        public void PlayItemSound(SoundEffectType soundEffect, float volumeScale = 1, bool loop = false)
        {
            AudioClip sound = effectDatabase.SoundEffectCollection[soundEffect];

            if (sound != null)
                PlayItemSound(sound, volumeScale, loop);
        }

        public void PlaySoundEffect(SoundEffectType soundEffect, float volumeScale = 1, bool loop = false)
        {
            AudioClip sound = effectDatabase.GetSoundEffect(soundEffect);

            if (sound != null)
                PlaySoundEffect(sound, volumeScale, loop);
        }

        public void PlayItemSound(AudioClip soundClip, float volumeScale = 1, bool loop = false)
        {
            if (soundClip == null || sfxSource == null)
                return;

            itemSource.loop = loop;
            itemSource.PlayOneShot(soundClip, volumeScale);
        }

        public void PlaySoundEffect(AudioClip soundClip, float volumeScale = 1, bool loop = false)
        {
            if (soundClip == null || sfxSource == null)
                return;

            sfxSource.loop = loop;
            sfxSource.PlayOneShot(soundClip, volumeScale);
        }

        public void PlayBackgroundMusic(BackgroundMusicType backgroundMusicType, bool loop = true, float volume = 1)
        {
            AudioClip bgm = musicDatabase.GetBackgroundMusic(backgroundMusicType);

            if (bgm != null)
                PlayMusic(bgm, loop, volume);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        private void AdjustMasterVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(MasterVolumeMixer, volume);
        }

        private void AdjustMusicVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(MusicVolumeMixer, volume);
        }

        private void AdjustSoundVolume(float value)
        {
            float volume = Mathf.Log10(value) * 20;
            audioMixer.SetFloat(SoundVolumeMixer, volume);
        }

        private void OnDestroy()
        {
            OnMasterChange -= AdjustMasterVolume;
            OnMusicChange -= AdjustMusicVolume;
            OnSoundChange -= AdjustSoundVolume;
        }
    }
}
