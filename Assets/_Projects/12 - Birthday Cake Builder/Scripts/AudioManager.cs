using System.Collections.Generic;
using UnityEngine;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Manages audio playback for sound effects and music.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        private Dictionary<string, AudioClip> soundLibrary;

        private const float DEFAULT_VOLUME = 0.7f;

        #region ==================================================================== Initialization

        protected override void Awake()
        {
            base.Awake();
            LoadSoundLibrary();
            SetupAudioSources();
        }

        /// <summary>
        /// Loads all audio clips from Resources/Audio folder.
        /// </summary>
        private void LoadSoundLibrary()
        {
            soundLibrary = new Dictionary<string, AudioClip>();
            AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio");

            foreach (AudioClip clip in clips)
            {
                soundLibrary[clip.name] = clip;
            }

            if (soundLibrary.Count == 0)
            {
                Debug.LogWarning("No audio clips found in Resources/Audio folder!");
            }
        }

        private void SetupAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = DEFAULT_VOLUME;
            }

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.volume = DEFAULT_VOLUME * 0.5f;
            }
        }

        #endregion ==================================================================

        #region ==================================================================== Sound Effects

        /// <summary>
        /// Plays a sound effect by name.
        /// Expected sounds: "drop", "success", "fail", "click", "screenshot"
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (!soundLibrary.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found in library!");
                return;
            }

            sfxSource.PlayOneShot(soundLibrary[soundName]);
        }

        /// <summary>
        /// Plays a sound effect with custom volume.
        /// </summary>
        public void PlaySound(string soundName, float volume)
        {
            if (!soundLibrary.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound '{soundName}' not found in library!");
                return;
            }

            sfxSource.PlayOneShot(soundLibrary[soundName], volume);
        }

        #endregion ==================================================================

        #region ==================================================================== Music

        /// <summary>
        /// Plays background music by name.
        /// </summary>
        public void PlayMusic(string musicName)
        {
            if (!soundLibrary.ContainsKey(musicName))
            {
                Debug.LogWarning($"Music '{musicName}' not found in library!");
                return;
            }

            musicSource.clip = soundLibrary[musicName];
            musicSource.Play();
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }

        #endregion ==================================================================
    }
}
