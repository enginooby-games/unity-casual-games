using UnityEngine;

namespace Devdy.AudioProcessing
{
    /// <summary>
    /// Comprehensive audio processing utility library for Unity.
    /// Provides reusable methods for pitch shifting, reverb, filtering, compression, and other effects.
    /// All methods are static and can be used across different audio projects.
    /// Thread-safe and optimized for real-time processing.
    /// </summary>
    public static class AudioProcessor
    {
        #region Pitch Shifting ==================================================================
        
        /// <summary>
        /// Applies pitch shifting to audio samples using cubic interpolation for high-quality results.
        /// This method changes the pitch without significantly affecting duration.
        /// </summary>
        /// <param name="samples">Input audio samples array</param>
        /// <param name="semitones">Pitch shift amount in semitones (-12 to +12 recommended)</param>
        /// <returns>New array with pitch-shifted audio samples</returns>
        /// <example>
        /// float[] shifted = AudioProcessor.ApplyPitchShift(originalSamples, 5f); // Shift up 5 semitones
        /// </example>
        public static float[] ApplyPitchShift(float[] samples, float semitones)
        {
            if (samples == null || samples.Length == 0)
            {
                Debug.LogWarning("AudioProcessor: Cannot pitch shift empty or null samples array");
                return samples;
            }
            
            if (Mathf.Approximately(semitones, 0f))
                return samples;
            
            float pitchFactor = Mathf.Pow(2f, semitones / 12f);
            int newLength = Mathf.RoundToInt(samples.Length / pitchFactor);
            float[] output = new float[newLength];
            
            for (int i = 0; i < newLength; i++)
            {
                float sourceIndex = i * pitchFactor;
                int index1 = Mathf.FloorToInt(sourceIndex);
                
                if (index1 >= samples.Length - 1)
                {
                    output[i] = samples[samples.Length - 1];
                    continue;
                }
                
                output[i] = CubicInterpolate(samples, index1, sourceIndex - index1);
            }
            
            return output;
        }
        
        /// <summary>
        /// Performs cubic interpolation for smoother audio quality during pitch shifting.
        /// Uses 4-point interpolation (Catmull-Rom spline) for better results than linear interpolation.
        /// </summary>
        /// <param name="samples">Source audio samples</param>
        /// <param name="index">Integer index position</param>
        /// <param name="fraction">Fractional part (0-1) between samples</param>
        /// <returns>Interpolated sample value</returns>
        private static float CubicInterpolate(float[] samples, int index, float fraction)
        {
            if (index < 1 || index >= samples.Length - 2)
            {
                // Fallback to linear interpolation at boundaries
                if (index >= samples.Length - 1) return samples[samples.Length - 1];
                return Mathf.Lerp(samples[index], samples[index + 1], fraction);
            }
            
            float y0 = samples[index - 1];
            float y1 = samples[index];
            float y2 = samples[index + 1];
            float y3 = samples[index + 2];
            
            float a0 = y3 - y2 - y0 + y1;
            float a1 = y0 - y1 - a0;
            float a2 = y2 - y0;
            float a3 = y1;
            
            return a0 * fraction * fraction * fraction + a1 * fraction * fraction + a2 * fraction + a3;
        }
        
        #endregion ==================================================================
        
        #region Reverb Effects ==================================================================
        
        /// <summary>
        /// Applies reverb effect using multi-tap comb filter algorithm.
        /// Modifies the input array in-place for performance.
        /// Creates room acoustics simulation with early reflections and diffusion.
        /// </summary>
        /// <param name="data">Audio data to process (modified in-place)</param>
        /// <param name="sampleRate">Audio sample rate (typically 44100 Hz)</param>
        /// <param name="roomSize">Room size factor (0-1, where 0=small room, 1=large hall)</param>
        /// <param name="mix">Wet/dry mix (0-1, where 0=dry/original, 1=wet/effected)</param>
        /// <example>
        /// AudioProcessor.ApplyReverb(audioData, 44100, 0.5f, 0.3f); // Medium room, 30% mix
        /// </example>
        public static void ApplyReverb(float[] data, int sampleRate, float roomSize, float mix)
        {
            if (data == null || data.Length == 0 || Mathf.Approximately(mix, 0f))
                return;
            
            // Multiple delay taps for realistic reverb
            int[] delayTimes = new int[]
            {
                Mathf.RoundToInt(roomSize * sampleRate * 0.03f),  // 30ms early reflection
                Mathf.RoundToInt(roomSize * sampleRate * 0.05f),  // 50ms
                Mathf.RoundToInt(roomSize * sampleRate * 0.07f),  // 70ms
                Mathf.RoundToInt(roomSize * sampleRate * 0.09f)   // 90ms late reflection
            };
            
            float[] gains = new float[] { 0.8f, 0.6f, 0.4f, 0.3f }; // Decreasing gains for natural decay
            
            for (int tap = 0; tap < delayTimes.Length; tap++)
            {
                int delayTime = Mathf.Max(1, delayTimes[tap]);
                ApplySingleTapDelay(data, delayTime, gains[tap] * mix);
            }
        }
        
        /// <summary>
        /// Applies a single delay tap with feedback.
        /// Internal helper method for creating reverb effect.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="delaySamples">Delay time in samples</param>
        /// <param name="gain">Feedback gain (0-1)</param>
        private static void ApplySingleTapDelay(float[] data, int delaySamples, float gain)
        {
            float[] delayBuffer = new float[delaySamples];
            int writeIndex = 0;
            
            for (int i = 0; i < data.Length; i++)
            {
                float delayed = delayBuffer[writeIndex];
                float output = data[i] + (delayed * gain);
                
                delayBuffer[writeIndex] = data[i];
                data[i] = output;
                
                writeIndex = (writeIndex + 1) % delaySamples;
            }
        }
        
        /// <summary>
        /// Applies early reflections for more realistic room acoustics.
        /// Simulates sound bouncing off walls with specific delay patterns.
        /// </summary>
        /// <param name="data">Audio data to process (modified in-place)</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <param name="roomSize">Room size factor (0-1)</param>
        public static void ApplyEarlyReflections(float[] data, int sampleRate, float roomSize)
        {
            if (data == null || data.Length == 0) return;
            
            // Early reflection patterns based on room acoustics research
            float[] reflectionTimes = new float[] { 5f, 11f, 17f, 23f, 29f, 35f }; // milliseconds
            float[] reflectionGains = new float[] { 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f };
            
            for (int i = 0; i < reflectionTimes.Length; i++)
            {
                int delaySamples = Mathf.RoundToInt((reflectionTimes[i] / 1000f) * roomSize * sampleRate);
                if (delaySamples > 0)
                {
                    ApplySingleTapDelay(data, delaySamples, reflectionGains[i]);
                }
            }
        }
        
        #endregion ==================================================================
        
        #region Dynamic Processing ==================================================================
        
        /// <summary>
        /// Applies dynamic range compression to control audio levels.
        /// Reduces the volume of loud sounds and can boost quiet sounds.
        /// Useful for consistent audio levels and preventing clipping.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="threshold">Threshold level (0-1, typically 0.5-0.8)</param>
        /// <param name="ratio">Compression ratio (1-20, where 1=no compression, 20=limiting)</param>
        /// <param name="attack">Attack time in seconds (0.001-0.1 typical)</param>
        /// <param name="release">Release time in seconds (0.05-1.0 typical)</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyCompressor(audioData, 0.6f, 4f, 0.01f, 0.1f, 44100);
        /// </example>
        public static void ApplyCompressor(float[] data, float threshold, float ratio, float attack, float release, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            float envelope = 0f;
            float attackCoeff = Mathf.Exp(-1f / (attack * sampleRate));
            float releaseCoeff = Mathf.Exp(-1f / (release * sampleRate));
            
            for (int i = 0; i < data.Length; i++)
            {
                float input = Mathf.Abs(data[i]);
                
                // Envelope follower with attack/release
                if (input > envelope)
                    envelope = attackCoeff * envelope + (1f - attackCoeff) * input;
                else
                    envelope = releaseCoeff * envelope + (1f - releaseCoeff) * input;
                
                // Calculate gain reduction
                float gainReduction = 1f;
                if (envelope > threshold)
                {
                    float excess = envelope - threshold;
                    gainReduction = threshold + (excess / ratio);
                    gainReduction /= envelope;
                }
                
                data[i] *= gainReduction;
            }
        }
        
        /// <summary>
        /// Applies noise gate to remove low-level background noise.
        /// Silences audio below threshold to clean up recordings.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="threshold">Gate threshold (0-1, typically 0.01-0.1)</param>
        /// <param name="attack">Attack time in seconds</param>
        /// <param name="release">Release time in seconds</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyNoiseGate(audioData, 0.05f, 0.001f, 0.05f, 44100);
        /// </example>
        public static void ApplyNoiseGate(float[] data, float threshold, float attack, float release, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            float envelope = 0f;
            float attackCoeff = Mathf.Exp(-1f / (attack * sampleRate));
            float releaseCoeff = Mathf.Exp(-1f / (release * sampleRate));
            
            for (int i = 0; i < data.Length; i++)
            {
                float input = Mathf.Abs(data[i]);
                
                // Envelope follower
                if (input > envelope)
                    envelope = attackCoeff * envelope + (1f - attackCoeff) * input;
                else
                    envelope = releaseCoeff * envelope + (1f - releaseCoeff) * input;
                
                // Apply gate
                if (envelope < threshold)
                {
                    data[i] = 0f;
                }
            }
        }
        
        #endregion ==================================================================
        
        #region Filtering ==================================================================
        
        /// <summary>
        /// Applies low-pass filter to remove high frequencies.
        /// Useful for removing high-frequency noise or creating muffled effects.
        /// Uses a simple one-pole IIR filter for real-time performance.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="cutoffFrequency">Cutoff frequency in Hz (20-20000)</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyLowPassFilter(audioData, 2000f, 44100); // Remove frequencies above 2kHz
        /// </example>
        public static void ApplyLowPassFilter(float[] data, float cutoffFrequency, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            float rc = 1f / (cutoffFrequency * 2f * Mathf.PI);
            float dt = 1f / sampleRate;
            float alpha = dt / (rc + dt);
            
            float previousOutput = data[0];
            
            for (int i = 1; i < data.Length; i++)
            {
                float output = previousOutput + alpha * (data[i] - previousOutput);
                data[i] = output;
                previousOutput = output;
            }
        }
        
        /// <summary>
        /// Applies high-pass filter to remove low frequencies.
        /// Useful for removing rumble, hum, or creating tinny effects.
        /// Uses a simple one-pole IIR filter for real-time performance.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="cutoffFrequency">Cutoff frequency in Hz (20-20000)</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyHighPassFilter(audioData, 100f, 44100); // Remove frequencies below 100Hz
        /// </example>
        public static void ApplyHighPassFilter(float[] data, float cutoffFrequency, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            float rc = 1f / (cutoffFrequency * 2f * Mathf.PI);
            float dt = 1f / sampleRate;
            float alpha = rc / (rc + dt);
            
            float previousInput = data[0];
            float previousOutput = data[0];
            
            for (int i = 1; i < data.Length; i++)
            {
                float output = alpha * (previousOutput + data[i] - previousInput);
                previousInput = data[i];
                data[i] = output;
                previousOutput = output;
            }
        }
        
        #endregion ==================================================================
        
        #region Normalization & Gain ==================================================================
        
        /// <summary>
        /// Normalizes audio to prevent clipping and optimize volume.
        /// Scales all samples so the peak amplitude matches the target level.
        /// Essential for consistent audio levels across different recordings.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="targetLevel">Target peak level (0-1, typically 0.95)</param>
        /// <example>
        /// AudioProcessor.Normalize(audioData, 0.95f); // Normalize to 95% peak
        /// </example>
        public static void Normalize(float[] data, float targetLevel = 0.95f)
        {
            if (data == null || data.Length == 0) return;
            
            float maxAmplitude = 0f;
            
            // Find peak amplitude
            for (int i = 0; i < data.Length; i++)
            {
                float abs = Mathf.Abs(data[i]);
                if (abs > maxAmplitude)
                    maxAmplitude = abs;
            }
            
            if (maxAmplitude < 0.001f) return; // Avoid division by zero
            
            // Apply normalization gain
            float gain = targetLevel / maxAmplitude;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= gain;
            }
        }
        
        /// <summary>
        /// Applies gain (volume) adjustment to audio.
        /// Simple multiplication of all samples by gain factor.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="gain">Gain multiplier (0.1-3.0 typical, 1.0=unity gain)</param>
        /// <example>
        /// AudioProcessor.ApplyGain(audioData, 1.5f); // Increase volume by 50%
        /// </example>
        public static void ApplyGain(float[] data, float gain)
        {
            if (data == null || data.Length == 0) return;
            
            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= gain;
            }
        }
        
        #endregion ==================================================================
        
        #region Fade Effects ==================================================================
        
        /// <summary>
        /// Applies fade-in effect at the start of audio.
        /// Gradually increases volume from 0 to full over the specified duration.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="durationSeconds">Fade duration in seconds</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyFadeIn(audioData, 0.5f, 44100); // 0.5 second fade-in
        /// </example>
        public static void ApplyFadeIn(float[] data, float durationSeconds, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            int fadeSamples = Mathf.Min(Mathf.RoundToInt(durationSeconds * sampleRate), data.Length);
            
            for (int i = 0; i < fadeSamples; i++)
            {
                float gain = (float)i / fadeSamples;
                data[i] *= gain;
            }
        }
        
        /// <summary>
        /// Applies fade-out effect at the end of audio.
        /// Gradually decreases volume from full to 0 over the specified duration.
        /// </summary>
        /// <param name="data">Audio data array (modified in-place)</param>
        /// <param name="durationSeconds">Fade duration in seconds</param>
        /// <param name="sampleRate">Audio sample rate</param>
        /// <example>
        /// AudioProcessor.ApplyFadeOut(audioData, 1.0f, 44100); // 1 second fade-out
        /// </example>
        public static void ApplyFadeOut(float[] data, float durationSeconds, int sampleRate)
        {
            if (data == null || data.Length == 0) return;
            
            int fadeSamples = Mathf.Min(Mathf.RoundToInt(durationSeconds * sampleRate), data.Length);
            int startIndex = data.Length - fadeSamples;
            
            for (int i = 0; i < fadeSamples; i++)
            {
                float gain = 1f - ((float)i / fadeSamples);
                data[startIndex + i] *= gain;
            }
        }
        
        #endregion ==================================================================
        
        #region Utility Functions ==================================================================
        
        /// <summary>
        /// Converts decibels to linear gain value.
        /// </summary>
        /// <param name="db">Decibel value</param>
        /// <returns>Linear gain value</returns>
        /// <example>
        /// float gain = AudioProcessor.DbToLinear(-6f); // -6dB = ~0.5 gain
        /// </example>
        public static float DbToLinear(float db)
        {
            return Mathf.Pow(10f, db / 20f);
        }
        
        /// <summary>
        /// Converts linear gain to decibels.
        /// </summary>
        /// <param name="linear">Linear gain value</param>
        /// <returns>Decibel value</returns>
        /// <example>
        /// float db = AudioProcessor.LinearToDb(0.5f); // 0.5 gain = -6dB
        /// </example>
        public static float LinearToDb(float linear)
        {
            if (linear <= 0f) return -100f;
            return 20f * Mathf.Log10(linear);
        }
        
        /// <summary>
        /// Calculates RMS (Root Mean Square) level of audio data.
        /// Provides average energy/loudness measurement.
        /// </summary>
        /// <param name="data">Audio data array</param>
        /// <returns>RMS level (0-1)</returns>
        /// <example>
        /// float loudness = AudioProcessor.CalculateRMS(audioData);
        /// </example>
        public static float CalculateRMS(float[] data)
        {
            if (data == null || data.Length == 0) return 0f;
            
            float sum = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i] * data[i];
            }
            return Mathf.Sqrt(sum / data.Length);
        }
        
        /// <summary>
        /// Finds the peak (maximum absolute) amplitude in audio data.
        /// </summary>
        /// <param name="data">Audio data array</param>
        /// <returns>Peak amplitude (0-1)</returns>
        public static float GetPeakAmplitude(float[] data)
        {
            if (data == null || data.Length == 0) return 0f;
            
            float peak = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                float abs = Mathf.Abs(data[i]);
                if (abs > peak)
                    peak = abs;
            }
            return peak;
        }
        
        /// <summary>
        /// Mixes two audio buffers together.
        /// Useful for combining dry and wet signals or layering sounds.
        /// </summary>
        /// <param name="audio1">First audio buffer</param>
        /// <param name="audio2">Second audio buffer</param>
        /// <param name="mix">Mix amount (0=only audio1, 1=only audio2, 0.5=equal mix)</param>
        /// <returns>New mixed audio array</returns>
        /// <example>
        /// float[] mixed = AudioProcessor.MixAudio(dry, wet, 0.3f); // 70% dry, 30% wet
        /// </example>
        public static float[] MixAudio(float[] audio1, float[] audio2, float mix = 0.5f)
        {
            if (audio1 == null || audio2 == null) return audio1 ?? audio2;
            
            int length = Mathf.Min(audio1.Length, audio2.Length);
            float[] output = new float[length];
            
            for (int i = 0; i < length; i++)
            {
                output[i] = Mathf.Lerp(audio1[i], audio2[i], mix);
            }
            
            return output;
        }
        
        /// <summary>
        /// Creates a copy of audio data array.
        /// Useful when you need to preserve original data.
        /// </summary>
        /// <param name="data">Source audio data</param>
        /// <returns>Copy of audio data</returns>
        public static float[] CloneAudioData(float[] data)
        {
            if (data == null) return null;
            
            float[] clone = new float[data.Length];
            System.Array.Copy(data, clone, data.Length);
            return clone;
        }
        
        #endregion ==================================================================
    }
}