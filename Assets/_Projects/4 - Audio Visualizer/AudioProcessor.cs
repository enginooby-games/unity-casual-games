using UnityEngine;
using System;

namespace AudioVisualizer.Core
{
    public class AudioProcessor : MonoBehaviour
    {
        private const int FREQUENCY_BINS = 512;
        private const float SMOOTHING_FACTOR = 0.7f;
        private const float BEAT_THRESHOLD = 0.1f;
        private const float LOW_FREQ_CUTOFF = 0.1f;
        private const float MID_FREQ_CUTOFF = 0.5f;
        private const float BEAT_COOLDOWN = 0.1f;
        private const float SPECTRUM_GAIN = 100f;

        private AudioSource audioSource;
        private float[] spectrumData;
        private float[] smoothedSpectrumData;
        private float[] previousSpectrumData;
        private float beatStrength;
        private float rmsValue;
        private float timeSinceLastBeat = float.MaxValue;
        private bool isInitialized = false;

        public event Action<float> OnBeatDetected;
        public event Action<float[]> OnSpectrumUpdated;

        public float BeatStrength => beatStrength;
        public float RmsValue => rmsValue;
        public float[] CurrentSpectrum => smoothedSpectrumData;
        public bool IsPlaying => audioSource != null && audioSource.isPlaying;

        private void Start()
        {
            InitializeAudioProcessor();
        }

        private void Update()
        {
            if (!isInitialized || !IsPlaying)
                return;

            timeSinceLastBeat += Time.deltaTime;
            UpdateFrequencyData();
            SmoothSpectrumData();
            CalculateBeatStrength();
            CalculateRmsValue();

            Debug.Log($"[AudioProcessor] RMS: {rmsValue}, Beat: {beatStrength}, Playing: {IsPlaying}");
            OnSpectrumUpdated?.Invoke(smoothedSpectrumData);
        }

        /// <summary>
        /// Initializes the audio processor by setting up arrays and AudioSource component.
        /// Configures AudioSource volume and spatial blend for proper spectrum analysis.
        /// Creates AudioSource if not present on GameObject.
        /// </summary>
        private void InitializeAudioProcessor()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Ensure audio source is configured for visualization
            audioSource.volume = 1f;
            audioSource.spatialBlend = 0f; // 2D audio

            spectrumData = new float[FREQUENCY_BINS];
            smoothedSpectrumData = new float[FREQUENCY_BINS];
            previousSpectrumData = new float[FREQUENCY_BINS];

            isInitialized = true;
            Debug.Log("[AudioProcessor] Initialized successfully");
        }

        private void UpdateFrequencyData()
        {
            audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
        }

        private void SmoothSpectrumData()
        {
            for (int i = 0; i < FREQUENCY_BINS; i++)
            {
                float boostedValue = Mathf.Min(spectrumData[i] * SPECTRUM_GAIN, 1f);
                smoothedSpectrumData[i] = 
                    boostedValue * (1f - SMOOTHING_FACTOR) + 
                    previousSpectrumData[i] * SMOOTHING_FACTOR;
                previousSpectrumData[i] = smoothedSpectrumData[i];
            }

            // Debug: Check if we have any data
            float maxValue = 0f;
            for (int i = 0; i < FREQUENCY_BINS; i++)
            {
                if (smoothedSpectrumData[i] > maxValue)
                    maxValue = smoothedSpectrumData[i];
            }
            Debug.Log($"[AudioProcessor] Smoothed Spectrum Max: {maxValue}");
        }

        private void CalculateBeatStrength()
        {
            float lowFreqAverage = GetFrequencyBandAverage(FrequencyBand.Low);
            beatStrength = Mathf.Lerp(beatStrength, lowFreqAverage, 0.1f);

            if (beatStrength > BEAT_THRESHOLD && timeSinceLastBeat >= BEAT_COOLDOWN)
            {
                OnBeatDetected?.Invoke(beatStrength);
                timeSinceLastBeat = 0f;
            }
        }

        private void CalculateRmsValue()
        {
            float sum = 0f;
            for (int i = 0; i < FREQUENCY_BINS; i++)
            {
                sum += spectrumData[i] * spectrumData[i];
            }
            rmsValue = Mathf.Sqrt(sum / FREQUENCY_BINS);
        }

        public float GetFrequencyBandAverage(FrequencyBand band)
        {
            int startIndex = 0;
            int endIndex = FREQUENCY_BINS;

            switch (band)
            {
                case FrequencyBand.Low:
                    endIndex = (int)(FREQUENCY_BINS * LOW_FREQ_CUTOFF);
                    break;
                case FrequencyBand.Mid:
                    startIndex = (int)(FREQUENCY_BINS * LOW_FREQ_CUTOFF);
                    endIndex = (int)(FREQUENCY_BINS * MID_FREQ_CUTOFF);
                    break;
                case FrequencyBand.High:
                    startIndex = (int)(FREQUENCY_BINS * MID_FREQ_CUTOFF);
                    break;
            }

            return GetFrequencyRangeAverage(startIndex, endIndex);
        }

        private float GetFrequencyRangeAverage(int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex > FREQUENCY_BINS || startIndex >= endIndex)
                return 0f;

            float sum = 0f;
            for (int i = startIndex; i < endIndex; i++)
            {
                sum += smoothedSpectrumData[i];
            }
            return sum / (endIndex - startIndex);
        }

        public float GetFrequencyValue(int index)
        {
            if (!isInitialized || index < 0 || index >= FREQUENCY_BINS)
                return 0f;

            return smoothedSpectrumData[index];
        }

        public void SetAudioClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.clip = clip;
            }
        }

        public void Play()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("[AudioProcessor] Playing");
            }
        }

        public void Stop()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Debug.Log("[AudioProcessor] Stopped");
            }
        }

        public void Pause()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                Debug.Log("[AudioProcessor] Paused");
            }
        }

        public void Resume()
        {
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                Debug.Log("[AudioProcessor] Resumed");
            }
        }
    }

    public enum FrequencyBand
    {
        Low,
        Mid,
        High
    }
}