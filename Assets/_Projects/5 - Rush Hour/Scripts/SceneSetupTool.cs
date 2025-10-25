using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace Devdy.RushHour.Editor
{
    /// <summary>
    /// Editor tool to automatically setup Rush Hour game scene structure.
    /// Creates all necessary GameObjects, components, and references.
    /// </summary>
    public class SceneSetupTool : EditorWindow
    {
        #region Window Setup
        [MenuItem("Tools/Rush Hour/Setup Scene")]
        public static void ShowWindow()
        {
            SceneSetupTool window = GetWindow<SceneSetupTool>("Rush Hour Scene Setup");
            window.minSize = new Vector2(400, 600);
        }
        #endregion

        #region Configuration
        private GameObject carPrefab;
        private GameObject coinPrefab;
        private Sprite playerSprite;
        private Sprite carSprite;
        private Sprite coinSprite;
        private Font tmpFont;
        #endregion

        #region GUI
        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Rush Hour Scene Setup Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This tool will create the complete scene structure for Rush Hour game.", MessageType.Info);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Prefabs (Optional - Will create if null)", EditorStyles.boldLabel);
            carPrefab = (GameObject)EditorGUILayout.ObjectField("Car Prefab", carPrefab, typeof(GameObject), false);
            coinPrefab = (GameObject)EditorGUILayout.ObjectField("Coin Prefab", coinPrefab, typeof(GameObject), false);
            
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Sprites (Optional)", EditorStyles.boldLabel);
            playerSprite = (Sprite)EditorGUILayout.ObjectField("Player Sprite", playerSprite, typeof(Sprite), false);
            carSprite = (Sprite)EditorGUILayout.ObjectField("Car Sprite", carSprite, typeof(Sprite), false);
            coinSprite = (Sprite)EditorGUILayout.ObjectField("Coin Sprite", coinSprite, typeof(Sprite), false);

            GUILayout.Space(20);
            
            if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Prefabs Only", GUILayout.Height(30)))
            {
                CreatePrefabs();
            }
            
            if (GUILayout.Button("Setup UI Only", GUILayout.Height(30)))
            {
                SetupUI();
            }
        }
        #endregion

        #region Main Setup
        /// <summary>
        /// Sets up complete scene with all GameObjects and components.
        /// </summary>
        private void SetupScene()
        {
            if (!EditorUtility.DisplayDialog("Setup Scene", 
                "This will create the complete scene structure. Continue?", "Yes", "Cancel"))
            {
                return;
            }

            CreatePrefabs();
            
            GameObject gameManager = CreateGameManager();
            GameObject player = CreatePlayer();
            GameObject carSpawner = CreateCarSpawner();
            GameObject coinSpawner = CreateCoinSpawner();
            GameObject powerupManager = CreatePowerupManager();
            GameObject[] trafficLights = CreateTrafficLights();
            GameObject canvas = SetupUI();

            LinkReferences(gameManager, player, carSpawner, coinSpawner, powerupManager, trafficLights, canvas);

            Debug.Log("✅ Rush Hour scene setup complete!");
            EditorUtility.DisplayDialog("Success", "Scene setup completed successfully!", "OK");
        }
        #endregion

        #region GameManager
        /// <summary>
        /// Creates GameManager GameObject with component.
        /// </summary>
        private GameObject CreateGameManager()
        {
            GameObject go = new GameObject("GameManager");
            GameManager gm = go.AddComponent<GameManager>();
            
            Undo.RegisterCreatedObjectUndo(go, "Create GameManager");
            
            return go;
        }
        #endregion

        #region Player
        /// <summary>
        /// Creates Player GameObject with all required components.
        /// </summary>
        private GameObject CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite != null ? playerSprite : CreateDefaultSprite(Color.blue);
            sr.sortingOrder = 10;
            
            CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;
            
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            PlayerController pc = player.AddComponent<PlayerController>();
            
            GameObject shieldEffect = new GameObject("ShieldEffect");
            shieldEffect.transform.SetParent(player.transform);
            shieldEffect.transform.localPosition = Vector3.zero;
            SpriteRenderer shieldSR = shieldEffect.AddComponent<SpriteRenderer>();
            shieldSR.sprite = CreateCircleSprite(Color.cyan);
            shieldSR.color = new Color(0, 1, 1, 0.3f);
            shieldSR.sortingOrder = 9;
            shieldEffect.transform.localScale = Vector3.one * 1.5f;
            shieldEffect.SetActive(false);
            
            GameObject magnetEffect = new GameObject("MagnetEffect");
            magnetEffect.transform.SetParent(player.transform);
            magnetEffect.transform.localPosition = Vector3.zero;
            SpriteRenderer magnetSR = magnetEffect.AddComponent<SpriteRenderer>();
            magnetSR.sprite = CreateCircleSprite(Color.red);
            magnetSR.color = new Color(1, 0, 0, 0.2f);
            magnetSR.sortingOrder = 8;
            magnetEffect.transform.localScale = Vector3.one * 4f;
            magnetEffect.SetActive(false);
            
            Undo.RegisterCreatedObjectUndo(player, "Create Player");
            
            return player;
        }
        #endregion

        #region CarSpawner
        /// <summary>
        /// Creates CarSpawner GameObject with lane configurations.
        /// </summary>
        private GameObject CreateCarSpawner()
        {
            GameObject spawner = new GameObject("CarSpawner");
            CarSpawner cs = spawner.AddComponent<CarSpawner>();
            
            SerializedObject so = new SerializedObject(cs);
            
            SerializedProperty laneConfigsProp = so.FindProperty("laneConfigs");
            laneConfigsProp.arraySize = 6;
            
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(0), 2f, 1f, 4f, 1, new Color(0.91f, 0.3f, 0.24f), 3);
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(1), 3.5f, 2f, 5f, -1, new Color(0.75f, 0.22f, 0.17f), 4);
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(2), 6f, 1f, 3f, 1, new Color(0.2f, 0.6f, 0.86f), 2);
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(3), 7.5f, 1f, 6f, -1, new Color(0.1f, 0.74f, 0.61f), 5);
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(4), 9f, 2f, 4.5f, 1, new Color(0.61f, 0.35f, 0.71f), 4);
            SetLaneConfig(laneConfigsProp.GetArrayElementAtIndex(5), 11.5f, 1f, 5.5f, -1, new Color(0.9f, 0.49f, 0.13f), 5);
            
            so.ApplyModifiedProperties();
            
            Undo.RegisterCreatedObjectUndo(spawner, "Create CarSpawner");
            
            return spawner;
        }

        private void SetLaneConfig(SerializedProperty prop, float row, float height, float speed, int direction, Color color, int danger)
        {
            prop.FindPropertyRelative("row").floatValue = row;
            prop.FindPropertyRelative("height").floatValue = height;
            prop.FindPropertyRelative("speed").floatValue = speed;
            prop.FindPropertyRelative("direction").intValue = direction;
            prop.FindPropertyRelative("color").colorValue = color;
            prop.FindPropertyRelative("dangerLevel").intValue = danger;
        }
        #endregion

        #region CoinSpawner
        /// <summary>
        /// Creates CoinSpawner GameObject with component.
        /// </summary>
        private GameObject CreateCoinSpawner()
        {
            GameObject spawner = new GameObject("CoinSpawner");
            CoinSpawner cs = spawner.AddComponent<CoinSpawner>();
            
            Undo.RegisterCreatedObjectUndo(spawner, "Create CoinSpawner");
            
            return spawner;
        }
        #endregion

        #region PowerupManager
        /// <summary>
        /// Creates PowerupManager GameObject with component.
        /// </summary>
        private GameObject CreatePowerupManager()
        {
            GameObject manager = new GameObject("PowerupManager");
            PowerupManager pm = manager.AddComponent<PowerupManager>();
            
            Undo.RegisterCreatedObjectUndo(manager, "Create PowerupManager");
            
            return manager;
        }
        #endregion

        #region TrafficLights
        /// <summary>
        /// Creates traffic light GameObjects for each lane.
        /// </summary>
        private GameObject[] CreateTrafficLights()
        {
            GameObject parent = new GameObject("TrafficLights");
            
            float[] lanes = { 2f, 3.5f, 6f, 7.5f, 9f, 11.5f };
            float[] heights = { 1f, 2f, 1f, 1f, 2f, 1f };
            bool[] startGreen = { true, false, true, false, false, true };
            
            GameObject[] lights = new GameObject[lanes.Length];
            
            for (int i = 0; i < lanes.Length; i++)
            {
                GameObject lightObj = new GameObject($"TrafficLight_{i}");
                lightObj.transform.SetParent(parent.transform);
                
                float yPos = (lanes[i] + heights[i] / 2f - 7f);
                lightObj.transform.position = new Vector3(7f, yPos, 0f);
                
                GameObject background = new GameObject("Background");
                background.transform.SetParent(lightObj.transform);
                background.transform.localPosition = Vector3.zero;
                SpriteRenderer bgSR = background.AddComponent<SpriteRenderer>();
                bgSR.sprite = CreateSquareSprite(new Color(0.17f, 0.24f, 0.31f));
                bgSR.sortingOrder = 5;
                background.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                
                GameObject light = new GameObject("Light");
                light.transform.SetParent(lightObj.transform);
                light.transform.localPosition = Vector3.zero;
                SpriteRenderer lightSR = light.AddComponent<SpriteRenderer>();
                lightSR.sprite = CreateCircleSprite(Color.green);
                lightSR.sortingOrder = 6;
                light.transform.localScale = Vector3.one * 0.4f;
                
                GameObject warning = new GameObject("WarningRing");
                warning.transform.SetParent(lightObj.transform);
                warning.transform.localPosition = Vector3.zero;
                SpriteRenderer warnSR = warning.AddComponent<SpriteRenderer>();
                warnSR.sprite = CreateCircleSprite(Color.yellow);
                warnSR.sortingOrder = 7;
                warning.transform.localScale = Vector3.one * 0.5f;
                warning.SetActive(false);
                
                GameObject textObj = new GameObject("CountdownText");
                textObj.transform.SetParent(lightObj.transform);
                textObj.transform.localPosition = Vector3.zero;
                TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
                tmp.text = "5";
                tmp.fontSize = 3;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.sortingOrder = 8;
                
                TrafficLight tl = lightObj.AddComponent<TrafficLight>();
                SerializedObject tlSO = new SerializedObject(tl);
                tlSO.FindProperty("laneRow").floatValue = lanes[i];
                tlSO.FindProperty("laneHeight").floatValue = heights[i];
                tlSO.FindProperty("startAsGreen").boolValue = startGreen[i];
                tlSO.FindProperty("lightSprite").objectReferenceValue = lightSR;
                tlSO.FindProperty("backgroundSprite").objectReferenceValue = bgSR;
                tlSO.FindProperty("warningRing").objectReferenceValue = warnSR;
                tlSO.FindProperty("countdownText").objectReferenceValue = tmp;
                tlSO.ApplyModifiedProperties();
                
                lights[i] = lightObj;
            }
            
            Undo.RegisterCreatedObjectUndo(parent, "Create Traffic Lights");
            
            return lights;
        }
        #endregion

        #region UI Setup
        /// <summary>
        /// Sets up complete UI canvas with all elements.
        /// </summary>
        private GameObject SetupUI()
        {
            GameObject canvas = new GameObject("Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvas.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            canvas.AddComponent<GraphicRaycaster>();
            
            GameObject statsPanel = CreateStatsPanel(canvas);
            GameObject powerupsPanel = CreatePowerupsPanel(canvas);
            GameObject gameOverPanel = CreateGameOverPanel(canvas);
            
            Undo.RegisterCreatedObjectUndo(canvas, "Create UI Canvas");
            
            return canvas;
        }

        private GameObject CreateStatsPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("StatsPanel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 100);
            rt.anchoredPosition = Vector2.zero;
            
            CreateStatText(panel, "CoinsText", new Vector2(-600, -50), "💰 0");
            CreateStatText(panel, "ScoreText", new Vector2(-300, -50), "🏆 0");
            CreateStatText(panel, "LivesText", new Vector2(0, -50), "❤️❤️❤️");
            CreateStatText(panel, "ComboText", new Vector2(300, -50), "");
            
            return panel;
        }

        private GameObject CreatePowerupsPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("PowerupsPanel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(600, 80);
            rt.anchoredPosition = new Vector2(0, -120);
            
            CreatePowerupButton(panel, "ShieldButton", new Vector2(-225, -40), "🛡️ Shield (15)");
            CreatePowerupButton(panel, "SlowMoButton", new Vector2(-75, -40), "⏱️ Slow (10)");
            CreatePowerupButton(panel, "MagnetButton", new Vector2(75, -40), "🧲 Magnet (8)");
            CreatePowerupButton(panel, "TeleportButton", new Vector2(225, -40), "⚡ Skip (12)");
            
            return panel;
        }

        private GameObject CreateGameOverPanel(GameObject canvas)
        {
            GameObject panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(canvas.transform, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            
            Image img = panel.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.9f);
            
            GameObject title = CreateUIText(panel, "Title", new Vector2(0, 150), "Game Over", 72);
            GameObject stats = CreateUIText(panel, "FinalStatsText", new Vector2(0, 0), "", 36);
            GameObject restartBtn = CreateButton(panel, "RestartButton", new Vector2(0, -150), "Play Again");
            
            panel.SetActive(false);
            
            return panel;
        }

        private GameObject CreateStatText(GameObject parent, string name, Vector2 pos, string text)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(250, 50);
            
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return go;
        }

        private GameObject CreatePowerupButton(GameObject parent, string name, Vector2 pos, string text)
        {
            GameObject btn = CreateButton(parent, name, pos, text);
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 60);
            
            GameObject cooldownBar = new GameObject("CooldownBar");
            cooldownBar.transform.SetParent(btn.transform, false);
            RectTransform barRT = cooldownBar.AddComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0);
            barRT.anchorMax = new Vector2(1, 0);
            barRT.pivot = new Vector2(0, 0);
            barRT.sizeDelta = new Vector2(0, 3);
            barRT.anchoredPosition = Vector2.zero;
            
            Image barImg = cooldownBar.AddComponent<Image>();
            barImg.color = Color.red;
            barImg.type = Image.Type.Filled;
            barImg.fillMethod = Image.FillMethod.Horizontal;
            
            return btn;
        }

        private GameObject CreateButton(GameObject parent, string name, Vector2 pos, string text)
        {
            GameObject btn = new GameObject(name);
            btn.transform.SetParent(parent.transform, false);
            RectTransform rt = btn.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(200, 60);
            
            Image img = btn.AddComponent<Image>();
            img.color = new Color(0.95f, 0.61f, 0.07f);
            
            Button button = btn.AddComponent<Button>();
            
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btn.transform, false);
            RectTransform txtRT = txtObj.AddComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;
            
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return btn;
        }

        private GameObject CreateUIText(GameObject parent, string name, Vector2 pos, string text, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(800, 100);
            
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            
            return go;
        }
        #endregion

        #region Prefab Creation
        /// <summary>
        /// Creates Car and Coin prefabs if not assigned.
        /// </summary>
        private void CreatePrefabs()
        {
            if (carPrefab == null)
            {
                carPrefab = CreateCarPrefab();
            }
            
            if (coinPrefab == null)
            {
                coinPrefab = CreateCoinPrefab();
            }
        }

        private GameObject CreateCarPrefab()
        {
            GameObject car = new GameObject("CarPrefab");
            
            SpriteRenderer sr = car.AddComponent<SpriteRenderer>();
            sr.sprite = carSprite != null ? carSprite : CreateDefaultSprite(Color.red);
            sr.sortingOrder = 5;
            
            BoxCollider2D collider = car.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(2f, 1f);
            collider.isTrigger = true;
            
            car.AddComponent<CarController>();
            car.tag = "Car";
            
            string path = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(car, $"{path}/Car.prefab");
            DestroyImmediate(car);
            
            Debug.Log($"✅ Created Car prefab at {path}/Car.prefab");
            return prefab;
        }

        private GameObject CreateCoinPrefab()
        {
            GameObject coin = new GameObject("CoinPrefab");
            
            SpriteRenderer sr = coin.AddComponent<SpriteRenderer>();
            sr.sprite = coinSprite != null ? coinSprite : CreateCircleSprite(Color.yellow);
            sr.sortingOrder = 5;
            
            CircleCollider2D collider = coin.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;
            
            GameObject textObj = new GameObject("ValueText");
            textObj.transform.SetParent(coin.transform);
            textObj.transform.localPosition = new Vector3(0, 0.5f, 0);
            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "x2";
            tmp.fontSize = 2;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.sortingOrder = 6;
            
            Coin coinComponent = coin.AddComponent<Coin>();
            SerializedObject so = new SerializedObject(coinComponent);
            so.FindProperty("coinSprite").objectReferenceValue = sr;
            so.FindProperty("valueText").objectReferenceValue = tmp;
            so.ApplyModifiedProperties();
            
            coin.tag = "Coin";
            
            string path = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, $"{path}/Coin.prefab");
            DestroyImmediate(coin);
            
            Debug.Log($"✅ Created Coin prefab at {path}/Coin.prefab");
            return prefab;
        }
        #endregion

        #region Reference Linking
        /// <summary>
        /// Links all references between GameObjects after creation.
        /// </summary>
        private void LinkReferences(GameObject gm, GameObject player, GameObject carSpawner, 
            GameObject coinSpawner, GameObject powerupManager, GameObject[] trafficLights, GameObject canvas)
        {
            GameManager gmComp = gm.GetComponent<GameManager>();
            SerializedObject gmSO = new SerializedObject(gmComp);
            gmSO.FindProperty("coinsText").objectReferenceValue = GameObject.Find("CoinsText")?.GetComponent<TextMeshProUGUI>();
            gmSO.FindProperty("scoreText").objectReferenceValue = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            gmSO.FindProperty("livesText").objectReferenceValue = GameObject.Find("LivesText")?.GetComponent<TextMeshProUGUI>();
            gmSO.FindProperty("comboText").objectReferenceValue = GameObject.Find("ComboText")?.GetComponent<TextMeshProUGUI>();
            gmSO.FindProperty("gameOverPanel").objectReferenceValue = GameObject.Find("GameOverPanel");
            gmSO.FindProperty("finalStatsText").objectReferenceValue = GameObject.Find("FinalStatsText")?.GetComponent<TextMeshProUGUI>();
            gmSO.ApplyModifiedProperties();
            
            CarSpawner csComp = carSpawner.GetComponent<CarSpawner>();
            SerializedObject csSO = new SerializedObject(csComp);
            csSO.FindProperty("carPrefab").objectReferenceValue = carPrefab;
            SerializedProperty trafficLightsProp = csSO.FindProperty("trafficLights");
            trafficLightsProp.arraySize = trafficLights.Length;
            for (int i = 0; i < trafficLights.Length; i++)
            {
                trafficLightsProp.GetArrayElementAtIndex(i).objectReferenceValue = trafficLights[i].GetComponent<TrafficLight>();
            }
            csSO.ApplyModifiedProperties();
            
            CoinSpawner coinSpawnerComp = coinSpawner.GetComponent<CoinSpawner>();
            SerializedObject coinSO = new SerializedObject(coinSpawnerComp);
            coinSO.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
            coinSO.ApplyModifiedProperties();
            
            PowerupManager pmComp = powerupManager.GetComponent<PowerupManager>();
            SerializedObject pmSO = new SerializedObject(pmComp);
            pmSO.FindProperty("player").objectReferenceValue = player.GetComponent<PlayerController>();
            pmSO.FindProperty("shieldButton").objectReferenceValue = GameObject.Find("ShieldButton")?.GetComponent<Button>();
            pmSO.FindProperty("slowmoButton").objectReferenceValue = GameObject.Find("SlowMoButton")?.GetComponent<Button>();
            pmSO.FindProperty("magnetButton").objectReferenceValue = GameObject.Find("MagnetButton")?.GetComponent<Button>();
            pmSO.FindProperty("teleportButton").objectReferenceValue = GameObject.Find("TeleportButton")?.GetComponent<Button>();
            pmSO.ApplyModifiedProperties();
            
            Debug.Log("✅ All references linked successfully!");
        }
        #endregion

        #region Helper Methods
        private Sprite CreateDefaultSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        private Sprite CreateCircleSprite(Color color)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];
            
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                    pixels[y * 64 + x] = dist < 30 ? color : Color.clear;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
        }

        private Sprite CreateSquareSprite(Color color)
        {
            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            tex.SetPixels(pixels);
            tex.Apply();
            
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }
        #endregion
    }
}