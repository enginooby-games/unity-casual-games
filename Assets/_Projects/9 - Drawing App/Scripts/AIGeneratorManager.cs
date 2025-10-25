using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

namespace Devdy.DrawingApp
{
    /// <summary>
    /// Manages AI image generation from user drawings using Stable Diffusion API.
    /// </summary>
    public class AIGeneratorManager : Singleton<AIGeneratorManager>
    {
        [SerializeField] private string apiKey = ""; // Get free key from https://stablediffusionapi.com
        [SerializeField] private string modelId;
        
        private const string API_URL = "https://stablediffusionapi.com/api/v4/dreambooth";
        // private const string IMG2IMG_URL = "https://stablediffusionapi.com/api/v3/img2img";
        private const string IMG2IMG_URL = "https://modelslab.com/api/v6/images/img2img";
        
        private bool isGenerating;

        /// <summary>
        /// Generates an AI image from the current drawing using image-to-image transformation.
        /// </summary>
        public void GenerateFromDrawing(Texture2D drawingTexture, string prompt, string apiKey, Action<Texture2D> onSuccess, Action<string> onError)
        {
            if (isGenerating)
            {
                onError?.Invoke("Already generating an image. Please wait.");
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                onError?.Invoke("API Key is not set. Please add your API key in AIGeneratorManager.");
                return;
            }

            StartCoroutine(GenerateImageCoroutine(drawingTexture, prompt, apiKey, onSuccess, onError));
        }

        private IEnumerator GenerateImageCoroutine(Texture2D drawingTexture, string prompt, string apiKey, Action<Texture2D> onSuccess, Action<string> onError)
        {
            isGenerating = true;

            // Convert texture to base64
            byte[] imageBytes = drawingTexture.EncodeToPNG();
            string base64Image = Convert.ToBase64String(imageBytes);

            // Prepare request data
            AIImageRequest requestData = new AIImageRequest
            {
                key = apiKey,
                prompt = string.IsNullOrEmpty(prompt) ? "Generate A realistic photograph of the object shown in the uploaded reference sketch image, with accurate proportions and textures — glossy metallic surface, subtle ambient shadows, natural lighting, shallow depth of field, high-resolution." : prompt,
                // init_image = "data:image/png;base64," + base64Image,
                init_image = base64Image,
                base64 = true,
                // init_image = base64Image,
                width = "512",
                height = "512",
                samples = "1",
                num_inference_steps = "30",
                safety_checker = "no",
                enhance_prompt = "yes",
                guidance_scale = 7.5f,
                strength = 0.7f,
                seed = null,
                model_id = modelId,
                // scheduler = "DDPMScheduler",           // ✅ REQUIRED FIELD
            };

            string jsonData = JsonUtility.ToJson(requestData);

            using (UnityWebRequest request = new UnityWebRequest(IMG2IMG_URL, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    isGenerating = false;
                    onError?.Invoke($"Network Error: {request.error}");
                    yield break;
                }

                // try
                // {
                    AIImageResponse response = JsonUtility.FromJson<AIImageResponse>(request.downloadHandler.text);

                    if (response.status == "error")
                    {
                        isGenerating = false;
                        onError?.Invoke($"API Error: {response.message}");
                        yield break;
                    }

                    if (response.status == "processing")
                    {
                        // Poll for result
                        yield return StartCoroutine(PollForResult(response.id, onSuccess, onError));
                    }
                    else if (response.status == "success" && response.output != null && response.output.Length > 0)
                    {
                        // Download the generated image
                        yield return StartCoroutine(DownloadImage(response.output[0], onSuccess, onError));
                    }
                    else
                    {
                        isGenerating = false;
                        onError?.Invoke("Unexpected response format from API.");
                    }
                // }
                // catch (Exception e)
                // {
                //     isGenerating = false;
                //     onError?.Invoke($"Error parsing response: {e.Message}");
                // }
            }
        }

        private IEnumerator PollForResult(string requestId, Action<Texture2D> onSuccess, Action<string> onError)
        {
            const string FETCH_URL = "https://stablediffusionapi.com/api/v4/dreambooth/fetch/";
            const int MAX_ATTEMPTS = 60;
            const float POLL_INTERVAL = 2f;

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                yield return new WaitForSeconds(POLL_INTERVAL);

                string fetchUrl = FETCH_URL + requestId;
                
                using (UnityWebRequest request = UnityWebRequest.Get(fetchUrl))
                {
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        continue;
                    }

                    // try
                    // {
                        AIImageResponse response = JsonUtility.FromJson<AIImageResponse>(request.downloadHandler.text);

                        if (response.status == "success" && response.output != null && response.output.Length > 0)
                        {
                            yield return StartCoroutine(DownloadImage(response.output[0], onSuccess, onError));
                            yield break;
                        }
                        else if (response.status == "error")
                        {
                            isGenerating = false;
                            onError?.Invoke($"Generation failed: {response.message}");
                            yield break;
                        }
                    // }
                    // catch (Exception e)
                    // {
                    //     Debug.LogWarning($"Error polling result: {e.Message}");
                    // }
                }
            }

            isGenerating = false;
            onError?.Invoke("Image generation timed out. Please try again.");
        }

        // private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onSuccess, Action<string> onError)
        // {
        //     using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        //     {
        //         yield return request.SendWebRequest();
        //
        //         if (request.result != UnityWebRequest.Result.Success)
        //         {
        //             isGenerating = false;
        //             onError?.Invoke($"Failed to download image: {request.error}");
        //             yield break;
        //         }
        //
        //         Texture2D texture = DownloadHandlerTexture.GetContent(request);
        //         isGenerating = false;
        //         onSuccess?.Invoke(texture);
        //     }
        // }
        
        private IEnumerator DownloadImage(string imageUrl, Action<Texture2D> onSuccess, Action<string> onError)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(imageUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    isGenerating = false;
                    onError?.Invoke($"Failed to download image: {request.error}");
                    yield break;
                }

                // Get downloaded data as text
                string text = request.downloadHandler.text;

                Texture2D texture = null;

                try
                {
                    // Check if it looks like Base64 (maybe huge string, no binary headers)
                    if (text.StartsWith("data:image/") || IsBase64String(text))
                    {
                        // Strip prefix if present
                        int commaIndex = text.IndexOf(',');
                        if (commaIndex >= 0)
                            text = text.Substring(commaIndex + 1);

                        byte[] imageBytes = Convert.FromBase64String(text);
                        texture = new Texture2D(512, 512); // size will be overwritten in LoadImage
                        texture.LoadImage(imageBytes);
                    }
                    else
                    {
                        // It’s not Base64: treat the download as an image binary
                        byte[] imageBytes = request.downloadHandler.data;
                        texture = new Texture2D(512,512);
                        texture.LoadImage(imageBytes);
                    }

                    isGenerating = false;
                    onSuccess?.Invoke(texture);
                }
                catch (Exception e)
                {
                    isGenerating = false;
                    onError?.Invoke($"Error decoding image: {e.Message}");
                }
            }
        }

// Helper method
        private bool IsBase64String(string s)
        {
            // Rough check: long string, only base64 chars + maybe newline
            s = s.Trim();
            return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[A-Za-z0-9\+/]*={0,3}$");
        }

        public bool IsGenerating() => isGenerating;

        #region API Data Classes ==================================================================

        [Serializable]
        private class AIImageRequest
        {
            public string key;
            public string prompt;
            public string init_image;
            public string width;
            public string height;
            public string samples;
            public string num_inference_steps;
            public string safety_checker;
            public string enhance_prompt;
            public float guidance_scale;
            public float strength;
            public object seed;
            public string model_id;
            public bool base64;
            // public string scheduler;
        }

        [Serializable]
        private class AIImageResponse
        {
            public string status;
            public string message;
            public string id;
            public string[] output;
            public string eta;
        }

        #endregion ==================================================================
    }
}