#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Automated scene setup tool for Birthday Cake Builder game.
    /// Creates all necessary GameObjects, managers, and UI elements.
    /// </summary>
    public static class BirthdayCakeSceneSetup
    {
        [MenuItem("Devdy/BirthdayCake/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            if (!EditorUtility.DisplayDialog(
                "Birthday Cake Scene Setup",
                "This will create all managers, UI, and game objects for the Birthday Cake Builder game.\n\nContinue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Setup 2D scene basics
            SetupSceneBasics();

            // Create managers
            SetupManagers();

            // Create game objects
            SetupGameObjects();

            // Create UI
            SetupUI();

            // Create resources folders
            CreateResourceFolders();

            Debug.Log("✅ Birthday Cake scene setup complete!");
            EditorUtility.DisplayDialog(
                "Setup Complete",
                "Scene setup finished!\n\n" +
                "Next steps:\n" +
                "1. Create LayerData assets in Resources/Layers/\n" +
                "2. Add audio clips to Resources/Audio/\n" +
                "3. Create candle prefab and assign to CakeBuilder\n" +
                "4. Setup confetti particle system with 'Confetti' tag\n" +
                "5. Assign UI references in UIManager inspector",
                "OK");
        }

        #region ==================================================================== Scene Basics

        private static void SetupSceneBasics()
        {
            // Create camera
            Camera camera = EditorUtilities.CreateMainCamera();
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.backgroundColor = new Color(0.95f, 0.85f, 0.95f); // Light pink background

            // Position camera
            camera.transform.position = new Vector3(0, 0, -10);

            Debug.Log("✓ Scene basics created (Camera)");
        }

        #endregion ==================================================================

        #region ==================================================================== Managers

        private static void SetupManagers()
        {
            // Create GameManager
            GameObject gameManagerGO = new GameObject("GameManager");
            GameManager gameManager = gameManagerGO.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gameManagerGO, "Create GameManager");

            // Create AudioManager
            EditorUtilities.CreateManager<AudioManager>();

            // Create UIManager (will be setup later with UI)
            GameObject uiManagerGO = new GameObject("UIManager");
            UIManager uiManager = uiManagerGO.AddComponent<UIManager>();
            Undo.RegisterCreatedObjectUndo(uiManagerGO, "Create UIManager");

            // Create CakeBuilder
            GameObject cakeBuilderGO = new GameObject("CakeBuilder");
            CakeBuilder cakeBuilder = cakeBuilderGO.AddComponent<CakeBuilder>();
            Undo.RegisterCreatedObjectUndo(cakeBuilderGO, "Create CakeBuilder");

            // Link references
            SerializedObject serializedGameManager = new SerializedObject(gameManager);
            serializedGameManager.FindProperty("cakeBuilder").objectReferenceValue = cakeBuilder;
            serializedGameManager.FindProperty("uiManager").objectReferenceValue = uiManager;
            serializedGameManager.ApplyModifiedProperties();

            Debug.Log("✓ Managers created (GameManager, AudioManager, UIManager, CakeBuilder)");
        }

        #endregion ==================================================================

        #region ==================================================================== Game Objects

        private static void SetupGameObjects()
        {
            // Create spawn point
            GameObject spawnPoint = EditorUtilities.CreateEmpty("SpawnPoint");
            spawnPoint.transform.position = new Vector3(0, 5f, 0);

            // Create cake base platform (2D)
            GameObject cakeBase = new GameObject("CakeBase");
            cakeBase.transform.position = new Vector3(0, -3f, 0);
            cakeBase.transform.localScale = new Vector3(10f, 0.5f, 1f);
            
            // Add sprite renderer for visual
            SpriteRenderer baseSr = cakeBase.AddComponent<SpriteRenderer>();
            baseSr.color = new Color(0.8f, 0.6f, 0.4f); // Brown color for base
            
            // Create a simple square sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            baseSr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            
            // Add 2D physics components
            BoxCollider2D baseCollider = cakeBase.AddComponent<BoxCollider2D>();
            baseCollider.size = new Vector2(1f, 1f);
            
            Rigidbody2D baseRb = cakeBase.AddComponent<Rigidbody2D>();
            baseRb.bodyType = RigidbodyType2D.Static;

            Undo.RegisterCreatedObjectUndo(cakeBase, "Create CakeBase");

            // Create confetti particle system
            GameObject confettiGO = new GameObject("Confetti");
            confettiGO.tag = "Confetti";
            ParticleSystem confetti = confettiGO.AddComponent<ParticleSystem>();
            
            // Configure confetti
            var main = confetti.main;
            main.startLifetime = 2f;
            main.startSpeed = 10f;
            main.startSize = 0.2f;
            main.maxParticles = 100;
            main.playOnAwake = false;

            var emission = confetti.emission;
            emission.rateOverTime = 50f;

            var shape = confetti.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;

            confettiGO.transform.position = new Vector3(0, 5f, 0);
            Undo.RegisterCreatedObjectUndo(confettiGO, "Create Confetti");

            // Link to CakeBuilder
            CakeBuilder cakeBuilder = Object.FindFirstObjectByType<CakeBuilder>();
            if (cakeBuilder != null)
            {
                SerializedObject serializedCakeBuilder = new SerializedObject(cakeBuilder);
                serializedCakeBuilder.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
                serializedCakeBuilder.FindProperty("cakeBase").objectReferenceValue = cakeBase.transform;
                serializedCakeBuilder.ApplyModifiedProperties();
            }

            Debug.Log("✓ Game objects created (SpawnPoint, CakeBase, Confetti)");
        }

        #endregion ==================================================================

        #region ==================================================================== UI Setup

        private static void SetupUI()
        {
            // Create Canvas
            GameObject canvasGO = EditorUtilities.CreateCanvas("Canvas");
            Canvas canvas = canvasGO.GetComponent<Canvas>();

            // Create Game UI
            GameObject gameUI = CreateGameUI(canvas.transform);

            // Create Victory Screen
            GameObject victoryScreen = CreateVictoryScreen(canvas.transform);

            // Create Fail Screen
            GameObject failScreen = CreateFailScreen(canvas.transform);

            // Link to UIManager
            UIManager uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager != null)
            {
                SerializedObject serializedUIManager = new SerializedObject(uiManager);
                serializedUIManager.FindProperty("gameUI").objectReferenceValue = gameUI;
                serializedUIManager.FindProperty("victoryScreen").objectReferenceValue = victoryScreen;
                serializedUIManager.FindProperty("failScreen").objectReferenceValue = failScreen;

                // Link Game UI elements
                TextMeshProUGUI layerCountText = gameUI.transform.Find("LayerCountText")?.GetComponent<TextMeshProUGUI>();
                serializedUIManager.FindProperty("layerCountText").objectReferenceValue = layerCountText;

                // Link Victory Screen elements
                TextMeshProUGUI victoryText = victoryScreen.transform.Find("VictoryText")?.GetComponent<TextMeshProUGUI>();
                Button screenshotButton = victoryScreen.transform.Find("ScreenshotButton")?.GetComponent<Button>();
                Button restartButton = victoryScreen.transform.Find("RestartButton")?.GetComponent<Button>();
                serializedUIManager.FindProperty("victoryText").objectReferenceValue = victoryText;
                serializedUIManager.FindProperty("screenshotButton").objectReferenceValue = screenshotButton;
                serializedUIManager.FindProperty("restartButton").objectReferenceValue = restartButton;

                // Link Fail Screen elements
                TextMeshProUGUI failText = failScreen.transform.Find("FailText")?.GetComponent<TextMeshProUGUI>();
                Button retryButton = failScreen.transform.Find("RetryButton")?.GetComponent<Button>();
                serializedUIManager.FindProperty("failText").objectReferenceValue = failText;
                serializedUIManager.FindProperty("retryButton").objectReferenceValue = retryButton;

                serializedUIManager.ApplyModifiedProperties();
            }

            Debug.Log("✓ UI created (Game UI, Victory Screen, Fail Screen)");
        }

        private static GameObject CreateGameUI(Transform canvasTransform)
        {
            GameObject gameUI = EditorUtilities.CreateEmpty("GameUI", canvasTransform);

            // Layer count text
            TextMeshProUGUI layerCountText = EditorUtilities.CreateText("LayerCountText", gameUI.transform, "Layer: 0/5");
            RectTransform layerCountRect = layerCountText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(layerCountRect, TextAnchor.UpperCenter);
            EditorUtilities.SetAnchoredPosition(layerCountRect, new Vector2(0, -50));
            layerCountText.fontSize = 48;
            layerCountText.fontStyle = FontStyles.Bold;

            return gameUI;
        }

        private static GameObject CreateVictoryScreen(Transform canvasTransform)
        {
            GameObject victoryScreen = EditorUtilities.CreateEmpty("VictoryScreen", canvasTransform);
            victoryScreen.SetActive(false);

            // Background panel
            Image panel = EditorUtilities.CreatePanel("Panel", victoryScreen.transform, new Color(0, 0, 0, 0.7f));

            // Victory text
            TextMeshProUGUI victoryText = EditorUtilities.CreateText("VictoryText", victoryScreen.transform, "🎉 Happy Birthday! 🎉");
            RectTransform victoryTextRect = victoryText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(victoryTextRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(victoryTextRect, new Vector2(0, 100));
            victoryText.fontSize = 64;
            victoryText.fontStyle = FontStyles.Bold;
            victoryText.color = new Color(1f, 0.84f, 0f); // Gold color

            // Screenshot button
            Button screenshotButton = EditorUtilities.CreateButton("ScreenshotButton", victoryScreen.transform, "📸 Screenshot");
            RectTransform screenshotRect = screenshotButton.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(screenshotRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(screenshotRect, new Vector2(0, -20));
            screenshotRect.sizeDelta = new Vector2(200, 70);

            // Restart button
            Button restartButton = EditorUtilities.CreateButton("RestartButton", victoryScreen.transform, "🔄 Play Again");
            RectTransform restartRect = restartButton.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(restartRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(restartRect, new Vector2(0, -110));
            restartRect.sizeDelta = new Vector2(200, 70);

            return victoryScreen;
        }

        private static GameObject CreateFailScreen(Transform canvasTransform)
        {
            GameObject failScreen = EditorUtilities.CreateEmpty("FailScreen", canvasTransform);
            failScreen.SetActive(false);

            // Background panel
            Image panel = EditorUtilities.CreatePanel("Panel", failScreen.transform, new Color(0.2f, 0, 0, 0.8f));

            // Fail text
            TextMeshProUGUI failText = EditorUtilities.CreateText("FailText", failScreen.transform, "Oops! The cake collapsed!");
            RectTransform failTextRect = failText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(failTextRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(failTextRect, new Vector2(0, 50));
            failText.fontSize = 52;
            failText.fontStyle = FontStyles.Bold;

            // Retry button
            Button retryButton = EditorUtilities.CreateButton("RetryButton", failScreen.transform, "🔄 Try Again");
            RectTransform retryRect = retryButton.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(retryRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(retryRect, new Vector2(0, -60));
            retryRect.sizeDelta = new Vector2(200, 70);

            return failScreen;
        }

        #endregion ==================================================================

        #region ==================================================================== Resource Folders

        private static void CreateResourceFolders()
        {
            string[] folders = new string[]
            {
                "Assets/Resources",
                "Assets/Resources/Layers",
                "Assets/Resources/Audio"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parentFolder = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
                    string newFolderName = System.IO.Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parentFolder, newFolderName);
                    Debug.Log($"✓ Created folder: {folder}");
                }
            }

            AssetDatabase.Refresh();
        }

        #endregion ==================================================================

        #region ==================================================================== Quick Actions

        [MenuItem("Devdy/BirthdayCake/Create Layer Data Template")]
        public static void CreateLayerDataTemplate()
        {
            string path = "Assets/Resources/Layers/NewLayerData.asset";
            LayerData layerData = ScriptableObject.CreateInstance<LayerData>();
            layerData.Type = LayerType.CakeLayer;
            layerData.Mass = 1f;
            layerData.Size = new Vector2(2f, 0.5f);

            AssetDatabase.CreateAsset(layerData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = layerData;

            Debug.Log($"✓ LayerData template created at: {path}");
        }

        [MenuItem("Devdy/BirthdayCake/Create Layer Prefab Template")]
        public static void CreateLayerPrefabTemplate()
        {
            // Create prefab folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Layers"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Layers");

            // Create layer GameObject
            GameObject layerGO = new GameObject("Layer_Template");
            layerGO.transform.localScale = new Vector3(2f, 0.5f, 1f);

            // Add sprite renderer with simple color
            SpriteRenderer sr = layerGO.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.86f, 0.71f); // Beige cake color
            
            // Create simple white sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

            // Add 2D box collider
            BoxCollider2D collider = layerGO.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f); // Will scale with transform

            // Save as prefab
            string prefabPath = "Assets/Prefabs/Layers/Layer_Template.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(layerGO, prefabPath);
            
            Object.DestroyImmediate(layerGO);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;

            Debug.Log($"✓ Layer prefab template created at: {prefabPath}");
            Debug.Log("Next steps:\n1. Duplicate this prefab for different layer types\n2. Change sprite/color for each variant\n3. Create LayerData assets and link them");
        }

        [MenuItem("Devdy/BirthdayCake/Create Candle Prefab Template")]
        public static void CreateCandlePrefabTemplate()
        {
            // Create candle GameObject
            GameObject candleGO = EditorUtilities.CreateEmpty("Candle");
            candleGO.tag = "Candle";
            candleGO.transform.localScale = new Vector3(0.2f, 0.6f, 1f);

            // Add sprite renderer (placeholder)
            SpriteRenderer sr = candleGO.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.9f, 0.7f);
            
            // Create simple white sprite
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

            // Add flame particle system
            GameObject flameGO = new GameObject("Flame");
            flameGO.transform.SetParent(candleGO.transform);
            flameGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            ParticleSystem flame = flameGO.AddComponent<ParticleSystem>();
            var main = flame.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 1f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.5f, 0f);
            main.playOnAwake = false;

            var emission = flame.emission;
            emission.rateOverTime = 20f;

            // Save as prefab
            string prefabPath = "Assets/Resources/Candle.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(candleGO, prefabPath);
            
            Object.DestroyImmediate(candleGO);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = prefab;

            Debug.Log($"✓ Candle prefab template created at: {prefabPath}");
        }

        [MenuItem("Devdy/BirthdayCake/Create Full Layer Set (5 Types)")]
        public static void CreateFullLayerSet()
        {
            if (!EditorUtility.DisplayDialog(
                "Create Full Layer Set",
                "This will create 5 layer prefabs and 5 LayerData assets with different colors:\n\n" +
                "• Cake (Beige)\n• Strawberry (Red)\n• Cream (Light Yellow)\n• Chocolate (Brown)\n• Cherry (Crimson)\n\n" +
                "Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            // Create folders
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Layers"))
                AssetDatabase.CreateFolder("Assets/Prefabs", "Layers");

            // Layer definitions
            var layerTypes = new[]
            {
                new { Name = "Cake", Type = LayerType.CakeLayer, Color = new Color(1f, 0.86f, 0.71f), Scale = new Vector3(2f, 0.5f, 1f), Mass = 1f },
                new { Name = "Strawberry", Type = LayerType.Strawberry, Color = new Color(1f, 0.39f, 0.39f), Scale = new Vector3(1.8f, 0.4f, 1f), Mass = 0.8f },
                new { Name = "Cream", Type = LayerType.Cream, Color = new Color(1f, 1f, 0.9f), Scale = new Vector3(2f, 0.3f, 1f), Mass = 0.6f },
                new { Name = "Chocolate", Type = LayerType.Chocolate, Color = new Color(0.55f, 0.27f, 0.07f), Scale = new Vector3(2f, 0.6f, 1f), Mass = 1.2f },
                new { Name = "Cherry", Type = LayerType.Cherry, Color = new Color(0.86f, 0.08f, 0.24f), Scale = new Vector3(1.5f, 0.4f, 1f), Mass = 0.5f }
            };

            foreach (var layer in layerTypes)
            {
                // Create prefab
                GameObject layerGO = new GameObject($"Layer_{layer.Name}");
                layerGO.transform.localScale = layer.Scale;

                SpriteRenderer sr = layerGO.AddComponent<SpriteRenderer>();
                sr.color = layer.Color;
                
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

                BoxCollider2D collider = layerGO.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 1f);

                string prefabPath = $"Assets/Prefabs/Layers/Layer_{layer.Name}.prefab";
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(layerGO, prefabPath);
                Object.DestroyImmediate(layerGO);

                // Create LayerData
                LayerData layerData = ScriptableObject.CreateInstance<LayerData>();
                layerData.Type = layer.Type;
                layerData.Prefab = prefab;
                layerData.Mass = layer.Mass;
                layerData.Size = new Vector2(layer.Scale.x, layer.Scale.y);

                string dataPath = $"Assets/Resources/Layers/LayerData_{layer.Name}.asset";
                AssetDatabase.CreateAsset(layerData, dataPath);

                Debug.Log($"✓ Created {layer.Name} layer: Prefab + LayerData");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("✅ Full layer set created! 5 prefabs and 5 LayerData assets ready to use.");
            EditorUtility.DisplayDialog(
                "Success!",
                "Created 5 layer types:\n\n" +
                "✓ Cake (Beige)\n✓ Strawberry (Red)\n✓ Cream (Light Yellow)\n✓ Chocolate (Brown)\n✓ Cherry (Crimson)\n\n" +
                "Check:\n• Assets/Prefabs/Layers/\n• Assets/Resources/Layers/",
                "OK");
        }

        #endregion ==================================================================
    }
}
#endif