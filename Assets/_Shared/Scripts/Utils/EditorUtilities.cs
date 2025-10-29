#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace Devdy
{
    /// <summary>
    /// Reusable editor utilities for quick scene setup across Unity projects.
    /// Provides methods for creating common UI elements, managers, and game objects.
    /// </summary>
    public static class EditorUtilities
    {
        #region Canvas Creation ==================================================================

        /// <summary>
        /// Creates a Canvas with EventSystem if it doesn't exist.
        /// </summary>
        [MenuItem("Devdy/Setup/Create Canvas")]
        public static GameObject CreateCanvas(string canvasName = "Canvas")
        {
            // Check if Canvas already exists
            Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
            {
                Debug.LogWarning($"Canvas already exists: {existingCanvas.name}");
                Selection.activeGameObject = existingCanvas.gameObject;
                return existingCanvas.gameObject;
            }

            // Create Canvas
            GameObject canvasGO = new GameObject(canvasName);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Configure CanvasScaler for responsive UI
            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(846, 512);
            scaler.matchWidthOrHeight = 0.5f;

            // Create EventSystem if it doesn't exist
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
            Selection.activeGameObject = canvasGO;
            
            Debug.Log($"Canvas created: {canvasName}");
            return canvasGO;
        }

        #endregion ==================================================================

        #region UI Elements Creation ==================================================================

        /// <summary>
        /// Creates a TextMeshProUGUI text element as a child of the specified parent.
        /// </summary>
        public static TextMeshProUGUI CreateText(string name, Transform parent = null, string initialText = "Text")
        {
            GameObject textGO = new GameObject(name);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            
            text.text = initialText;
            text.fontSize = 36;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform rectTransform = textGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);

            if (parent != null)
            {
                textGO.transform.SetParent(parent, false);
            }
            else
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    textGO.transform.SetParent(canvas.transform, false);
                }
            }

            Undo.RegisterCreatedObjectUndo(textGO, $"Create {name}");
            return text;
        }

        /// <summary>
        /// Creates a Button with TextMeshProUGUI label.
        /// </summary>
        public static Button CreateButton(string name, Transform parent = null, string buttonText = "Button")
        {
            GameObject buttonGO = new GameObject(name);
            Button button = buttonGO.AddComponent<Button>();
            Image image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            RectTransform rectTransform = buttonGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 60);

            // Create button text
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            if (parent != null)
            {
                buttonGO.transform.SetParent(parent, false);
            }
            else
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    buttonGO.transform.SetParent(canvas.transform, false);
                }
            }

            Undo.RegisterCreatedObjectUndo(buttonGO, $"Create {name}");
            return button;
        }

        /// <summary>
        /// Creates a UI Panel (Image component).
        /// </summary>
        public static Image CreatePanel(string name, Transform parent = null, Color? backgroundColor = null)
        {
            GameObject panelGO = new GameObject(name);
            Image image = panelGO.AddComponent<Image>();
            image.color = backgroundColor ?? new Color(0f, 0f, 0f, 0.8f);

            RectTransform rectTransform = panelGO.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            if (parent != null)
            {
                panelGO.transform.SetParent(parent, false);
            }
            else
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    panelGO.transform.SetParent(canvas.transform, false);
                }
            }

            Undo.RegisterCreatedObjectUndo(panelGO, $"Create {name}");
            return image;
        }

        #endregion ==================================================================

        #region Manager Creation ==================================================================

        /// <summary>
        /// Creates a manager GameObject with the specified component type.
        /// Ensures only one instance exists in the scene.
        /// </summary>
        public static T CreateManager<T>(string managerName = null) where T : MonoBehaviour
        {
            // Check if manager already exists
            T existingManager = Object.FindFirstObjectByType<T>();
            if (existingManager != null)
            {
                Debug.LogWarning($"{typeof(T).Name} already exists in the scene.");
                Selection.activeGameObject = existingManager.gameObject;
                return existingManager;
            }

            string name = managerName ?? typeof(T).Name;
            GameObject managerGO = new GameObject(name);
            T manager = managerGO.AddComponent<T>();

            Undo.RegisterCreatedObjectUndo(managerGO, $"Create {name}");
            Selection.activeGameObject = managerGO;
            
            Debug.Log($"Manager created: {name}");
            return manager;
        }

        #endregion ==================================================================

        #region Game Object Helpers ==================================================================

        /// <summary>
        /// Creates an empty GameObject with optional parent.
        /// </summary>
        public static GameObject CreateEmpty(string name, Transform parent = null)
        {
            GameObject go = new GameObject(name);
            
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        /// <summary>
        /// Creates a 2D sprite GameObject.
        /// </summary>
        public static SpriteRenderer CreateSprite(string name, Sprite sprite = null, Transform parent = null)
        {
            GameObject spriteGO = new GameObject(name);
            SpriteRenderer spriteRenderer = spriteGO.AddComponent<SpriteRenderer>();
            
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }

            if (parent != null)
            {
                spriteGO.transform.SetParent(parent, false);
            }

            Undo.RegisterCreatedObjectUndo(spriteGO, $"Create {name}");
            return spriteRenderer;
        }

        /// <summary>
        /// Creates a 3D cube primitive.
        /// </summary>
        public static GameObject CreateCube(string name, Vector3 position = default, Transform parent = null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;

            if (parent != null)
            {
                cube.transform.SetParent(parent, false);
            }

            Undo.RegisterCreatedObjectUndo(cube, $"Create {name}");
            return cube;
        }

        #endregion ==================================================================

        #region Scene Setup Helpers ==================================================================

        /// <summary>
        /// Creates main camera if it doesn't exist.
        /// </summary>
        [MenuItem("Devdy/Setup/Create Main Camera")]
        public static Camera CreateMainCamera()
        {
            Camera existingCamera = Camera.main;
            if (existingCamera != null)
            {
                Debug.LogWarning("Main Camera already exists in the scene.");
                Selection.activeGameObject = existingCamera.gameObject;
                return existingCamera;
            }

            GameObject cameraGO = new GameObject("Main Camera");
            Camera camera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();

            Undo.RegisterCreatedObjectUndo(cameraGO, "Create Main Camera");
            Selection.activeGameObject = cameraGO;
            
            Debug.Log("Main Camera created");
            return camera;
        }

        /// <summary>
        /// Creates a directional light if none exists.
        /// </summary>
        [MenuItem("Devdy/Setup/Create Directional Light")]
        public static Light CreateDirectionalLight()
        {
            Light existingLight = Object.FindFirstObjectByType<Light>();
            if (existingLight != null && existingLight.type == LightType.Directional)
            {
                Debug.LogWarning("Directional Light already exists in the scene.");
                Selection.activeGameObject = existingLight.gameObject;
                return existingLight;
            }

            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

            Undo.RegisterCreatedObjectUndo(lightGO, "Create Directional Light");
            Selection.activeGameObject = lightGO;
            
            Debug.Log("Directional Light created");
            return light;
        }

        /// <summary>
        /// Quick setup for a basic 2D scene with Camera and Canvas.
        /// </summary>
        [MenuItem("Devdy/Setup/Quick 2D Scene Setup")]
        public static void QuickSetup2D()
        {
            Camera camera = CreateMainCamera();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);

            CreateCanvas();

            Debug.Log("2D Scene setup complete!");
        }

        /// <summary>
        /// Quick setup for a basic 3D scene with Camera, Light, and Ground plane.
        /// </summary>
        [MenuItem("Devdy/Setup/Quick 3D Scene Setup")]
        public static void QuickSetup3D()
        {
            Camera camera = CreateMainCamera();
            camera.transform.position = new Vector3(0, 1, -10);
            camera.backgroundColor = new Color(0.2f, 0.3f, 0.5f);

            CreateDirectionalLight();

            // Create ground plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(10, 1, 10);
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

            Debug.Log("3D Scene setup complete!");
        }

        #endregion ==================================================================

        #region Utility Methods ==================================================================

        /// <summary>
        /// Sets the anchored position of a RectTransform.
        /// </summary>
        public static void SetAnchoredPosition(RectTransform rectTransform, Vector2 position)
        {
            Undo.RecordObject(rectTransform, "Set Anchored Position");
            rectTransform.anchoredPosition = position;
        }

        /// <summary>
        /// Sets the anchor preset for a RectTransform.
        /// </summary>
        public static void SetAnchorPreset(RectTransform rectTransform, TextAnchor preset)
        {
            Undo.RecordObject(rectTransform, "Set Anchor Preset");
            
            switch (preset)
            {
                case TextAnchor.UpperLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    break;
                case TextAnchor.UpperCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    break;
                case TextAnchor.UpperRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    break;
                case TextAnchor.MiddleCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                // Add more cases as needed
            }
        }

        /// <summary>
        /// Finds or creates a parent GameObject for organizing hierarchy.
        /// </summary>
        public static GameObject FindOrCreateParent(string parentName)
        {
            GameObject parent = GameObject.Find(parentName);
            if (parent == null)
            {
                parent = CreateEmpty(parentName);
            }
            return parent;
        }

        #endregion ==================================================================
    }
}
#endif
