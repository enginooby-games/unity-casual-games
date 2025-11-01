#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace Devdy.FallingCatch
{
    /// <summary>
    /// Editor script to automatically set up the FallingCatch game scene.
    /// </summary>
    public static class FallingCatchSceneSetup
    {
        [MenuItem("Devdy/FallingCatch/Setup Complete Scene")]
        public static void SetupCompleteScene()
        {
            Debug.Log("Starting FallingCatch scene setup...");

            // 1. Setup 2D Camera
            SetupCamera();

            // 2. Create Managers
            CreateManagers();

            // 3. Create UI
            CreateUI();

            // 4. Create Player
            CreatePlayer();

            // 5. Create FallingObject Prefab
            CreateFallingObjectPrefab();

            // 6. Create ConfigData Asset
            CreateConfigDataAsset();

            Debug.Log("✅ FallingCatch scene setup complete! Check Console for next steps.");
            EditorUtility.DisplayDialog("Setup Complete", 
                "FallingCatch scene has been set up!\n\n" +
                "Next steps:\n" +
                "1. Assign ConfigData asset to GameManager\n" +
                "2. Assign FallingObject prefab to SpawnManager\n" +
                "3. Press Play to test!", 
                "OK");
        }

        #region Camera Setup
        private static void SetupCamera()
        {
            Camera camera = EditorUtilities.CreateMainCamera();
            camera.orthographic = true;
            camera.orthographicSize = 6;
            camera.backgroundColor = new Color(0.1f, 0.15f, 0.2f);
            camera.transform.position = new Vector3(0, 0, -10);

            Debug.Log("✓ Camera configured for 2D");
        }
        #endregion

        #region Managers Creation
        private static void CreateManagers()
        {
            // GameManager
            var gameManager = EditorUtilities.CreateManager<GameManager>("GameManager");
            Debug.Log("✓ GameManager created");

            // SpawnManager
            var spawnManager = EditorUtilities.CreateManager<SpawnManager>("SpawnManager");
            
            // Create SpawnParent for organization
            GameObject spawnParent = EditorUtilities.CreateEmpty("SpawnParent");
            SerializedObject so = new SerializedObject(spawnManager);
            so.FindProperty("spawnParent").objectReferenceValue = spawnParent.transform;
            so.ApplyModifiedProperties();
            
            Debug.Log("✓ SpawnManager created with SpawnParent");

            // UIManager
            var uiManager = EditorUtilities.CreateManager<UIManager>("UIManager");
            Debug.Log("✓ UIManager created");
        }
        #endregion

        #region UI Creation
        private static void CreateUI()
        {
            // Create Canvas
            GameObject canvas = EditorUtilities.CreateCanvas("Canvas");
            Transform canvasTransform = canvas.transform;

            // Get UIManager to assign references
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            SerializedObject uiManagerSO = new SerializedObject(uiManager);

            // Score Text (Top-Left)
            TextMeshProUGUI scoreText = EditorUtilities.CreateText("ScoreText", canvasTransform, "Score: 0");
            RectTransform scoreRect = scoreText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(scoreRect, TextAnchor.UpperLeft);
            EditorUtilities.SetAnchoredPosition(scoreRect, new Vector2(100, -30));
            scoreText.fontSize = 32;
            scoreText.alignment = TextAlignmentOptions.Left;
            uiManagerSO.FindProperty("scoreText").objectReferenceValue = scoreText;

            // Health Text (Top-Right)
            TextMeshProUGUI healthText = EditorUtilities.CreateText("HealthText", canvasTransform, "Health: 3");
            RectTransform healthRect = healthText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(healthRect, TextAnchor.UpperRight);
            EditorUtilities.SetAnchoredPosition(healthRect, new Vector2(-100, -30));
            healthText.fontSize = 32;
            healthText.alignment = TextAlignmentOptions.Right;
            uiManagerSO.FindProperty("healthText").objectReferenceValue = healthText;

            // Timer Text (Top-Center)
            TextMeshProUGUI timerText = EditorUtilities.CreateText("TimerText", canvasTransform, "Time: 60s");
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(timerRect, TextAnchor.UpperCenter);
            EditorUtilities.SetAnchoredPosition(timerRect, new Vector2(0, -30));
            timerText.fontSize = 32;
            timerText.alignment = TextAlignmentOptions.Center;
            uiManagerSO.FindProperty("timerText").objectReferenceValue = timerText;

            // Game Over Panel
            Image gameOverPanel = EditorUtilities.CreatePanel("GameOverPanel", canvasTransform, new Color(0, 0, 0, 0.9f));
            uiManagerSO.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel.gameObject;

            // Final Score Text (inside panel)
            TextMeshProUGUI finalScoreText = EditorUtilities.CreateText("FinalScoreText", gameOverPanel.transform, "Final Score: 0");
            RectTransform finalScoreRect = finalScoreText.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(finalScoreRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(finalScoreRect, new Vector2(0, 50));
            finalScoreText.fontSize = 48;
            finalScoreText.color = Color.yellow;
            uiManagerSO.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;

            // Restart Button (inside panel)
            Button restartButton = EditorUtilities.CreateButton("RestartButton", gameOverPanel.transform, "Restart");
            RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
            EditorUtilities.SetAnchorPreset(buttonRect, TextAnchor.MiddleCenter);
            EditorUtilities.SetAnchoredPosition(buttonRect, new Vector2(0, -50));
            buttonRect.sizeDelta = new Vector2(200, 70);
            uiManagerSO.FindProperty("restartButton").objectReferenceValue = restartButton;

            // Set Game Over Panel inactive by default
            gameOverPanel.gameObject.SetActive(false);

            uiManagerSO.ApplyModifiedProperties();
            Debug.Log("✓ UI created and assigned to UIManager");
        }
        #endregion

        #region Player Creation
        private static void CreatePlayer()
        {
            GameObject playerGO = new GameObject("Player");
            
            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = playerGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            spriteRenderer.color = Color.cyan;
            
            // Scale to look like a basket
            playerGO.transform.localScale = new Vector3(2f, 0.5f, 1f);
            playerGO.transform.position = new Vector3(0, -4.5f, 0);

            // Add BoxCollider2D (Trigger)
            BoxCollider2D collider = playerGO.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            // Add PlayerController
            playerGO.AddComponent<PlayerController>();

            // Set Tag
            if (!TagExists("Player"))
            {
                AddTag("Player");
            }
            playerGO.tag = "Player";

            Undo.RegisterCreatedObjectUndo(playerGO, "Create Player");
            Debug.Log("✓ Player created at bottom of screen");
        }
        #endregion

        #region Prefab Creation
        private static void CreateFallingObjectPrefab()
        {
            // Create temporary GameObject
            GameObject fallingObjectGO = new GameObject("FallingObject");
            
            // Add SpriteRenderer
            SpriteRenderer spriteRenderer = fallingObjectGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            spriteRenderer.color = Color.green;

            // Add Rigidbody2D
            Rigidbody2D rb = fallingObjectGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Add CircleCollider2D
            CircleCollider2D collider = fallingObjectGO.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;

            // Add FallingObject script
            fallingObjectGO.AddComponent<FallingObject>();

            // Save as prefab
            string prefabPath = "Assets/FallingObjectPrefab.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fallingObjectGO, prefabPath);
            
            // Assign to SpawnManager
            var spawnManager = Object.FindFirstObjectByType<SpawnManager>();
            SerializedObject so = new SerializedObject(spawnManager);
            so.FindProperty("fallingObjectPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();

            // Delete temporary GameObject
            Object.DestroyImmediate(fallingObjectGO);

            Debug.Log($"✓ FallingObject prefab created at: {prefabPath}");
        }
        #endregion

        #region Asset Creation
        private static void CreateConfigDataAsset()
        {
            string assetPath = "Assets/FallingCatchConfig.asset";
            
            // Check if asset already exists
            ConfigData existingConfig = AssetDatabase.LoadAssetAtPath<ConfigData>(assetPath);
            if (existingConfig != null)
            {
                Debug.LogWarning($"ConfigData already exists at {assetPath}");
                AssignConfigToGameManager(existingConfig);
                return;
            }

            // Create new ConfigData
            ConfigData config = ScriptableObject.CreateInstance<ConfigData>();
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"✓ ConfigData created at: {assetPath}");

            AssignConfigToGameManager(config);
        }

        private static void AssignConfigToGameManager(ConfigData config)
        {
            var gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                SerializedObject so = new SerializedObject(gameManager);
                so.FindProperty("config").objectReferenceValue = config;
                so.ApplyModifiedProperties();
                Debug.Log("✓ ConfigData assigned to GameManager");
            }
        }
        #endregion

        #region Helper Methods
        private static bool TagExists(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    return true;
            }
            return false;
        }

        private static void AddTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(0);
            newTag.stringValue = tag;
            tagManager.ApplyModifiedProperties();
            
            Debug.Log($"✓ Tag '{tag}' added to project");
        }
        #endregion
    }
}
#endif