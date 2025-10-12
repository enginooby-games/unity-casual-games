using UnityEngine;
using AudioVisualizer.Core;

namespace AudioVisualizer.Visualization
{
    public class SpectrumVisualizer : MonoBehaviour
    {
        [SerializeField] private AudioProcessor audioProcessor;
        [SerializeField] private Material spectrumMaterial;
        [SerializeField] private int barCount = 64;
        [SerializeField] private float barSpacing = 0.1f;
        /// <summary>Maximum bar height scale multiplier (increased to 50 for better visibility of quiet audio)</summary>
        [SerializeField] private float maxBarHeight = 50f;

        /// <summary>Controls height variance between bars (0-1). Lower = more uniform, Higher = more varied</summary>
        [SerializeField] private float heightVariance = 0.5f;
        [SerializeField] private Color lowFreqColor = Color.red;
        [SerializeField] private Color midFreqColor = Color.green;
        [SerializeField] private Color highFreqColor = Color.blue;

        private const float SCALE_INTERPOLATION_SPEED = 0.3f;
        private const float MIN_BAR_HEIGHT = 0.05f;

        private GameObject[] bars;
        private Renderer[] barRenderers;
        private Material[] barMaterials;
        private Vector3[] currentBarScales;

        private void Start()
        {
            InitializeBars();
            SubscribeToAudioEvents();
        }

        private void Update()
        {
            if (audioProcessor == null || bars == null)
                return;

            float[] spectrum = audioProcessor.CurrentSpectrum;
            if (spectrum == null || spectrum.Length == 0)
                return;

            OnSpectrumUpdated(spectrum);
        }

        private void OnDestroy()
        {
            UnsubscribeFromAudioEvents();
        }

        private void InitializeBars()
        {
            bars = new GameObject[barCount];
            barRenderers = new Renderer[barCount];
            barMaterials = new Material[barCount];
            currentBarScales = new Vector3[barCount];

            for (int i = 0; i < barCount; i++)
            {
                GameObject bar = CreateBar(i);
                bars[i] = bar;
                barRenderers[i] = bar.GetComponent<Renderer>();
                
                barMaterials[i] = new Material(spectrumMaterial ?? new Material(Shader.Find("Standard")));
                barRenderers[i].material = barMaterials[i];
                
                currentBarScales[i] = bar.transform.localScale;
            }

            Debug.Log($"[SpectrumVisualizer] Initialized {barCount} bars");
        }

        private GameObject CreateBar(int index)
        {
            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = $"Bar_{index}";
            bar.transform.SetParent(transform);

            float xPosition = (index - barCount / 2) * barSpacing;
            bar.transform.localPosition = new Vector3(xPosition, 0, 0);
            bar.transform.localScale = new Vector3(0.08f, MIN_BAR_HEIGHT, 0.08f);

            Collider collider = bar.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }

            return bar;
        }

        private void SubscribeToAudioEvents()
        {
            if (audioProcessor != null)
            {
                audioProcessor.OnSpectrumUpdated += OnSpectrumUpdated;
                Debug.Log("[SpectrumVisualizer] Subscribed to audio events");
            }
            else
            {
                Debug.LogError("[SpectrumVisualizer] Audio processor not assigned");
            }
        }

        private void UnsubscribeFromAudioEvents()
        {
            if (audioProcessor != null)
            {
                audioProcessor.OnSpectrumUpdated -= OnSpectrumUpdated;
            }
        }

        private void OnSpectrumUpdated(float[] spectrumData)
        {
            if (spectrumData == null || bars == null)
                return;

            // Calculate normalization factor based on overall spectrum max
            float maxSpectrumValue = 0f;
            for (int i = 0; i < spectrumData.Length; i++)
            {
                if (spectrumData[i] > maxSpectrumValue)
                    maxSpectrumValue = spectrumData[i];
            }

            // Prevent division by zero
            if (maxSpectrumValue < 0.0001f)
                maxSpectrumValue = 0.0001f;

            int binsPerBar = Mathf.Max(1, spectrumData.Length / barCount);

            for (int i = 0; i < barCount; i++)
            {
                if (bars[i] == null)
                    continue;

                // Calculate average of multiple bins for this bar
                int startIndex = i * binsPerBar;
                int endIndex = Mathf.Min((i + 1) * binsPerBar, spectrumData.Length);
                
                float frequencyValue = 0f;
                for (int j = startIndex; j < endIndex; j++)
                {
                    frequencyValue += spectrumData[j];
                }
                frequencyValue /= (endIndex - startIndex);
                
                // Normalize against max spectrum value with variance control
                float normalizedValue = Mathf.Clamp01(frequencyValue / maxSpectrumValue);
                
                // Apply height variance: lerp between full normalization and 1.0
                // heightVariance 0 = all bars at MIN_BAR_HEIGHT (minimum variance)
                // heightVariance 1 = full normalization (maximum variance)
                float adjustedValue = Mathf.Lerp(1f, normalizedValue, heightVariance);
                adjustedValue = Mathf.Clamp01(adjustedValue);
                
                float targetHeight = Mathf.Max(MIN_BAR_HEIGHT, adjustedValue * maxBarHeight);

                UpdateBarHeight(i, targetHeight);
                UpdateBarColor(i, adjustedValue);

                if (i == 0)
                {
                    Debug.Log($"[SpectrumVisualizer] Bar 0: Freq={normalizedValue}, Adjusted={adjustedValue}, Height={targetHeight}");
                }
                if (i == barCount / 2)
                {
                    Debug.Log($"[SpectrumVisualizer] Bar Mid: Adjusted={adjustedValue}, Height={targetHeight}, Index={i}");
                }
                if (i == barCount - 1)
                {
                    Debug.Log($"[SpectrumVisualizer] Bar Last: Adjusted={adjustedValue}, Height={targetHeight}, Index={i}");
                }
            }
        }

        private void UpdateBarHeight(int index, float targetHeight)
        {
            if (bars[index] == null)
                return;

            Vector3 currentScale = bars[index].transform.localScale;
            float newHeight = Mathf.Lerp(currentScale.y, targetHeight, SCALE_INTERPOLATION_SPEED);
            currentScale.y = newHeight;
            bars[index].transform.localScale = currentScale;

            if (index == 0)
            {
                Debug.Log($"[SpectrumVisualizer] Bar 0 - Target: {targetHeight}, Current: {newHeight}");
            }
        }

        private void UpdateBarColor(int index, float frequencyValue)
        {
            if (barMaterials[index] == null)
                return;

            Color targetColor = GetColorForFrequency(index);
            Color finalColor = Color.Lerp(Color.black, targetColor, frequencyValue);
            barMaterials[index].color = finalColor;

            if (index == 0 || index == barCount - 1)
            {
                Debug.Log($"[SpectrumVisualizer] Bar {index}: Color={finalColor}, Target={targetColor}, Freq={frequencyValue}");
            }
        }

        private Color GetColorForFrequency(int index)
        {
            float normalizedPosition = index / (float)barCount;
            
            if (normalizedPosition < 0.33f)
                return lowFreqColor;
            else if (normalizedPosition < 0.66f)
                return midFreqColor;
            else
                return highFreqColor;
        }
    }
}
