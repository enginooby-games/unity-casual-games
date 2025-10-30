using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;

namespace Devdy.VoiceModulator
{
    /// <summary>
    /// Handles audio file import/export operations for both WebGL and mobile platforms.
    /// Supports WAV format conversion and file system operations.
    /// </summary>
    public class AudioFileHandler : MonoBehaviour
    {
        #region Private Fields ==================================================================
        
        private VoiceModulatorManager manager; // Reference to voice modulator manager
        
        private const int WAV_HEADER_SIZE = 44;
        
        #endregion ==================================================================
        
        #region Unity Lifecycle ==================================================================
        
        private void Start()
        {
            manager = VoiceModulatorManager.Instance;
        }
        
        #endregion ==================================================================
        
        #region Import Audio ==================================================================
        
        /// <summary>
        /// Imports audio from a file path (for mobile platforms).
        /// </summary>
        public void ImportAudioFromPath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Audio file not found: {filePath}");
                return;
            }
            
            StartCoroutine(LoadAudioFile(filePath));
        }
        
        /// <summary>
        /// Imports audio from Resources folder.
        /// </summary>
        public void ImportAudioFromResources(string resourcePath)
        {
            AudioClip clip = Resources.Load<AudioClip>(resourcePath);
            
            if (clip == null)
            {
                Debug.LogError($"Failed to load audio from Resources: {resourcePath}");
                return;
            }
            
            manager.ImportAudioFile(clip);
        }
        
        private IEnumerator LoadAudioFile(string filePath)
        {
            string url = "file://" + filePath;
            
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to load audio file: {www.error}");
                    yield break;
                }
                
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                
                if (clip != null)
                {
                    manager.ImportAudioFile(clip);
                    Debug.Log($"Audio file imported successfully: {filePath}");
                }
                else
                {
                    Debug.LogError("Failed to extract audio clip from file.");
                }
            }
        }
        
        #endregion ==================================================================
        
        #region Export Audio ==================================================================
        
        /// <summary>
        /// Exports processed audio to WAV file.
        /// </summary>
        public void ExportToWAV(string fileName = "processed_audio.wav")
        {
            AudioClip clip = manager.ExportProcessedAudio();
            
            if (clip == null)
            {
                Debug.LogError("No audio clip to export!");
                return;
            }
            
#if UNITY_WEBGL && !UNITY_EDITOR
            // For WebGL: Convert to WAV and trigger browser download
            byte[] wavData = ConvertAudioClipToWAV(clip);
            DownloadFileWebGL(fileName, wavData);
#else
            // For mobile/standalone: Save to persistent data path
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            SaveWAVFile(filePath, clip);
            Debug.Log($"Audio exported to: {filePath}");
#endif
        }
        
        /// <summary>
        /// Saves AudioClip as WAV file to specified path.
        /// </summary>
        private void SaveWAVFile(string filePath, AudioClip clip)
        {
            byte[] wavData = ConvertAudioClipToWAV(clip);
            File.WriteAllBytes(filePath, wavData);
        }
        
        /// <summary>
        /// Converts AudioClip to WAV format byte array.
        /// </summary>
        private byte[] ConvertAudioClipToWAV(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);
            
            short[] intData = new short[samples.Length];
            byte[] bytesData = new byte[samples.Length * 2];
            
            // Convert float samples to 16-bit PCM
            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * 32767f);
            }
            
            // Convert to byte array
            Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
            
            // Create WAV file with header
            byte[] wav = new byte[WAV_HEADER_SIZE + bytesData.Length];
            
            // WAV header
            int sampleRate = clip.frequency;
            int channels = clip.channels;
            int byteRate = sampleRate * channels * 2;
            
            // RIFF header
            wav[0] = (byte)'R';
            wav[1] = (byte)'I';
            wav[2] = (byte)'F';
            wav[3] = (byte)'F';
            
            // File size
            int fileSize = wav.Length - 8;
            wav[4] = (byte)(fileSize & 0xFF);
            wav[5] = (byte)((fileSize >> 8) & 0xFF);
            wav[6] = (byte)((fileSize >> 16) & 0xFF);
            wav[7] = (byte)((fileSize >> 24) & 0xFF);
            
            // WAVE header
            wav[8] = (byte)'W';
            wav[9] = (byte)'A';
            wav[10] = (byte)'V';
            wav[11] = (byte)'E';
            
            // fmt subchunk
            wav[12] = (byte)'f';
            wav[13] = (byte)'m';
            wav[14] = (byte)'t';
            wav[15] = (byte)' ';
            
            // Subchunk1 size (16 for PCM)
            wav[16] = 16;
            wav[17] = 0;
            wav[18] = 0;
            wav[19] = 0;
            
            // Audio format (1 = PCM)
            wav[20] = 1;
            wav[21] = 0;
            
            // Number of channels
            wav[22] = (byte)channels;
            wav[23] = 0;
            
            // Sample rate
            wav[24] = (byte)(sampleRate & 0xFF);
            wav[25] = (byte)((sampleRate >> 8) & 0xFF);
            wav[26] = (byte)((sampleRate >> 16) & 0xFF);
            wav[27] = (byte)((sampleRate >> 24) & 0xFF);
            
            // Byte rate
            wav[28] = (byte)(byteRate & 0xFF);
            wav[29] = (byte)((byteRate >> 8) & 0xFF);
            wav[30] = (byte)((byteRate >> 16) & 0xFF);
            wav[31] = (byte)((byteRate >> 24) & 0xFF);
            
            // Block align
            int blockAlign = channels * 2;
            wav[32] = (byte)blockAlign;
            wav[33] = 0;
            
            // Bits per sample
            wav[34] = 16;
            wav[35] = 0;
            
            // data subchunk
            wav[36] = (byte)'d';
            wav[37] = (byte)'a';
            wav[38] = (byte)'t';
            wav[39] = (byte)'a';
            
            // Subchunk2 size
            int dataSize = bytesData.Length;
            wav[40] = (byte)(dataSize & 0xFF);
            wav[41] = (byte)((dataSize >> 8) & 0xFF);
            wav[42] = (byte)((dataSize >> 16) & 0xFF);
            wav[43] = (byte)((dataSize >> 24) & 0xFF);
            
            // Copy audio data
            Buffer.BlockCopy(bytesData, 0, wav, WAV_HEADER_SIZE, bytesData.Length);
            
            return wav;
        }
        
        /// <summary>
        /// Triggers browser download for WebGL builds.
        /// </summary>
        private void DownloadFileWebGL(string fileName, byte[] data)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string base64Data = Convert.ToBase64String(data);
            Application.ExternalEval($@"
                var link = document.createElement('a');
                link.download = '{fileName}';
                link.href = 'data:audio/wav;base64,{base64Data}';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            ");
            Debug.Log($"Audio download triggered: {fileName}");
#endif
        }
        
        #endregion ==================================================================
        
        #region Platform-Specific File Picker ==================================================================
        
        /// <summary>
        /// Opens native file picker (requires platform-specific implementation).
        /// For mobile: Use native plugins like NativeFilePicker
        /// For WebGL: Use HTML5 file input
        /// </summary>
        public void OpenFilePicker()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OpenFilePickerWebGL();
#elif UNITY_ANDROID || UNITY_IOS
            Debug.LogWarning("Native file picker requires platform-specific plugin (e.g., NativeFilePicker).");
            // Example: NativeFilePicker.PickFile() for mobile
#else
            Debug.Log("File picker not implemented for this platform.");
#endif
        }
        
        private void OpenFilePickerWebGL()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Application.ExternalEval(@"
                var input = document.createElement('input');
                input.type = 'file';
                input.accept = 'audio/*';
                input.onchange = function(e) {
                    var file = e.target.files[0];
                    var reader = new FileReader();
                    reader.onload = function(event) {
                        // Send audio data to Unity
                        console.log('Audio file selected:', file.name);
                    };
                    reader.readAsArrayBuffer(file);
                };
                input.click();
            ");
#endif
        }
        
        #endregion ==================================================================
    }
}
