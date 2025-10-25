using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;

namespace Devdy.DrawingApp
{
    public class PromptGenerator : MonoBehaviour
    {
        public enum StyleType
        {
            Realistic,
            Cartoon,
            Anime,
            Sketchy,
            Minimalistic
        }

        public static class PromptTemplates
        {
            public static readonly Dictionary<StyleType, string> StyleToPrompt = new Dictionary<StyleType, string>
            {
                {
                    StyleType.Realistic,
                    "Using the uploaded sketch as a guide, generate a photorealistic image of the {object} with the exact same colour scheme as the drawing (for example: blue eyes, yellow head, etc.). The object should look like a real-life photograph, with accurate lighting, textures, reflections, ambient shadows and high resolution. No text or watermark."
                },
                {
                    StyleType.Cartoon,
                    "Using the uploaded sketch as a guide, generate a cartoon-style illustration of the {object}, preserving the exact colour scheme from the drawing."
                },
                {
                    StyleType.Anime,
                    "Using the uploaded sketch as a guide, create an anime-style artwork of the {object}, keeping the same colour palette as the drawing."
                },
                {
                    StyleType.Sketchy,
                    "Using the uploaded sketch as a guide, render the {object} in a sketchy hand-drawn style, while preserving the exact colours from the drawing (for example: blue eyes, yellow head). The image should look like an artist’s pencil/ink sketch with loose lines, visible texture, and the same colour accents as in the original sketch. High resolution."
                },
                {
                    StyleType.Minimalistic,
                    "Using the uploaded sketch as a guide, generate a minimalistic design of the {object} with the same colour scheme as the drawing."
                }
            };
        }

        [Header("UI References")] 
        [SerializeField] private TMP_InputField objectInputField;
        [SerializeField] private TMP_InputField apiKeyInputField;
        [SerializeField] private GameObject loadingIndicator; // Loading spinner/indicator
        [SerializeField] private TextMeshProUGUI statusText; // Status message text
        
        [SerializeField] private Button realisticButton;
        [SerializeField] private Button cartoonButton;
        [SerializeField] private Button animeButton;
        [SerializeField] private Button sketchyButton;
        [SerializeField] private Button minimalisticButton;

        private void Awake()
        {
            // Attach button handlers
            realisticButton.onClick.AddListener(() => OnStyleSelected(StyleType.Realistic));
            cartoonButton.onClick.AddListener(() => OnStyleSelected(StyleType.Cartoon));
            animeButton.onClick.AddListener(() => OnStyleSelected(StyleType.Anime));
            sketchyButton.onClick.AddListener(() => OnStyleSelected(StyleType.Sketchy));
            minimalisticButton.onClick.AddListener(() => OnStyleSelected(StyleType.Minimalistic));
        }

        private void OnStyleSelected(StyleType style)
        {
            if (DrawingManager.Instance.IsGeneratingAI()) return;

            if (apiKeyInputField.text.Trim().Length == 0)
            {
                statusText.text = "Need modelslab API key!";
                return;
            }
            
            string objectName = objectInputField.text.Trim();
            if (string.IsNullOrEmpty(objectName))
            {
                statusText.text = "Object name is empty. Please enter a valid object.";
                return;
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(true);
            }
            
            if (statusText != null)
            {
                statusText.text = "Generating AI image...";
            }

            // Get prompt template
            if (!PromptTemplates.StyleToPrompt.TryGetValue(style, out string template))
            {
                Debug.LogError("No template found for style: " + style);
                return;
            }

            // Replace placeholder
            string fullPrompt = template.Replace("{object}", objectName);

            Debug.Log("Generated Prompt: " + fullPrompt);
            DrawingManager.Instance.GenerateAIImage(fullPrompt, apiKeyInputField.text.Trim());
        }
        
        public void OnAIGenerationComplete(bool success, string message)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
            
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = success ? Color.green : Color.red;
            }
        }
    }
}