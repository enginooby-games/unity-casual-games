using UnityEngine;
using UnityEditor;

namespace Devdy.RushHour.Editor
{
    /// <summary>
    /// Debug tool window for Rush Hour game during runtime.
    /// Provides quick access to game state manipulation and testing features.
    /// </summary>
    public class GameDebugTool : EditorWindow
    {
        #region Window Setup
        [MenuItem("Tools/Rush Hour/Debug Tool")]
        public static void ShowWindow()
        {
            GameDebugTool window = GetWindow<GameDebugTool>("Rush Hour Debug");
            window.minSize = new Vector2(350, 700);
        }
        #endregion

        #region Private Fields
        private Vector2 scrollPosition;
        private int coinsToAdd = 50;
        private int scoreToadd = 100;
        private int livesToSet = 3;
        #endregion

        #region GUI
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawGameStateSection();
            DrawQuickActionsSection();
            DrawPowerupsSection();
            DrawSpawnersSection();
            DrawPlayerSection();
            DrawStatisticsSection();
            
            EditorGUILayout.EndScrollView();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use debug features", MessageType.Warning);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
        #endregion

        #region Header
        private void DrawHeader()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Rush Hour Debug Tool", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Debug and test game features in Play Mode", MessageType.Info);
            GUILayout.Space(10);
        }
        #endregion

        #region Game State Section
        private void DrawGameStateSection()
        {
            EditorGUILayout.LabelField("Game State", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Not in Play Mode", MessageType.None);
                GUILayout.Space(10);
                return;
            }

            if (GameManager.Instance == null)
            {
                EditorGUILayout.HelpBox("GameManager not found in scene", MessageType.Error);
                GUILayout.Space(10);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Current Coins", GameManager.Instance.CurrentCoins);
            EditorGUILayout.IntField("Current Score", GameManager.Instance.CurrentScore);
            EditorGUILayout.IntField("Current Lives", GameManager.Instance.CurrentLives);
            EditorGUILayout.IntField("Combo Count", GameManager.Instance.ComboCount);
            EditorGUILayout.IntField("Max Combo", GameManager.Instance.MaxCombo);
            EditorGUILayout.IntField("Levels Passed", GameManager.Instance.LevelsPassed);
            EditorGUILayout.Toggle("Is Game Over", GameManager.Instance.IsGameOver);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion

        #region Quick Actions Section
        private void DrawQuickActionsSection()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            if (!Application.isPlaying || GameManager.Instance == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Restart Game");
                GUILayout.Button("Trigger Game Over");
                GUILayout.Button("Complete Level");
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
                return;
            }

            if (GUILayout.Button("Restart Game", GUILayout.Height(30)))
            {
                GameManager.Instance.RestartGame();
                Debug.Log("🔄 Game Restarted");
            }

            EditorGUI.BeginDisabledGroup(GameManager.Instance.IsGameOver);
            if (GUILayout.Button("Trigger Game Over"))
            {
                while (GameManager.Instance.CurrentLives > 0)
                {
                    GameManager.Instance.LoseLife();
                }
                Debug.Log("💀 Game Over Triggered");
            }

            if (GUILayout.Button("Complete Level"))
            {
                GameManager.Instance.CompleteLevel();
                Debug.Log("✅ Level Completed");
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion

        #region Powerups Section
        private void DrawPowerupsSection()
        {
            EditorGUILayout.LabelField("Powerups", EditorStyles.boldLabel);

            if (!Application.isPlaying || PowerupManager.Instance == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Activate Shield");
                GUILayout.Button("Activate Slow-Mo");
                GUILayout.Button("Activate Magnet");
                GUILayout.Button("Teleport to Goal");
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Shield Active", PowerupManager.Instance.IsShieldActive);
            EditorGUILayout.Toggle("Slow-Mo Active", PowerupManager.Instance.IsSlowMoActive);
            EditorGUILayout.Toggle("Magnet Active", PowerupManager.Instance.IsMagnetActive);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(GameManager.Instance.IsGameOver);
            
            if (GUILayout.Button("Activate Shield (Free)"))
            {
                PowerupManager.Instance.BuyPowerup(PowerupType.Shield);
                Debug.Log("🛡️ Shield Activated");
            }

            if (GUILayout.Button("Activate Slow-Mo (Free)"))
            {
                PowerupManager.Instance.BuyPowerup(PowerupType.SlowMo);
                Debug.Log("⏱️ Slow-Mo Activated");
            }

            if (GUILayout.Button("Activate Magnet (Free)"))
            {
                PowerupManager.Instance.BuyPowerup(PowerupType.Magnet);
                Debug.Log("🧲 Magnet Activated");
            }

            if (GUILayout.Button("Teleport to Goal (Free)"))
            {
                PowerupManager.Instance.BuyPowerup(PowerupType.Teleport);
                Debug.Log("⚡ Teleported");
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion

        #region Spawners Section
        private void DrawSpawnersSection()
        {
            EditorGUILayout.LabelField("Spawners", EditorStyles.boldLabel);

            CarSpawner carSpawner = FindObjectOfType<CarSpawner>();
            CoinSpawner coinSpawner = FindObjectOfType<CoinSpawner>();

            if (!Application.isPlaying || carSpawner == null || coinSpawner == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Clear All Cars");
                GUILayout.Button("Clear All Coins");
                GUILayout.Button("Spawn 10 Cars");
                GUILayout.Button("Spawn 10 Coins");
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            // EditorGUILayout.IntField("Active Cars", carSpawner.GetActiveCarCount());
            // EditorGUILayout.IntField("Active Coins", coinSpawner.GetActiveCoinCount());
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(GameManager.Instance.IsGameOver);

            if (GUILayout.Button("Clear All Cars"))
            {
                carSpawner.ClearAllCars();
                Debug.Log("🚗 All cars cleared");
            }

            if (GUILayout.Button("Clear All Coins"))
            {
                coinSpawner.ClearAllCoins();
                Debug.Log("🪙 All coins cleared");
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion

        #region Player Section
        private void DrawPlayerSection()
        {
            EditorGUILayout.LabelField("Player Controls", EditorStyles.boldLabel);

            PlayerController player = FindObjectOfType<PlayerController>();

            if (!Application.isPlaying || player == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Respawn Player");
                GUILayout.Button("Kill Player");
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            Vector2Int gridPos = player.GetGridPosition();
            EditorGUILayout.Vector2IntField("Grid Position", gridPos);
            EditorGUILayout.Toggle("Has Shield", player.HasShield());
            EditorGUILayout.Toggle("Has Magnet", player.HasMagnet());
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(GameManager.Instance.IsGameOver);

            if (GUILayout.Button("Respawn Player"))
            {
                player.RespawnPlayer();
                Debug.Log("♻️ Player Respawned");
            }

            if (GUILayout.Button("Kill Player"))
            {
                GameManager.Instance.LoseLife();
                Debug.Log("💀 Player Killed");
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion

        #region Statistics Section
        private void DrawStatisticsSection()
        {
            EditorGUILayout.LabelField("Modify Statistics", EditorStyles.boldLabel);

            if (!Application.isPlaying || GameManager.Instance == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Coins to Add", 50);
                GUILayout.Button("Add Coins");
                EditorGUILayout.IntField("Score to Add", 100);
                GUILayout.Button("Add Score");
                EditorGUILayout.IntField("Set Lives", 3);
                GUILayout.Button("Set Lives");
                EditorGUI.EndDisabledGroup();
                GUILayout.Space(10);
                return;
            }

            EditorGUI.BeginDisabledGroup(GameManager.Instance.IsGameOver);

            coinsToAdd = EditorGUILayout.IntField("Coins to Add", coinsToAdd);
            if (GUILayout.Button($"Add {coinsToAdd} Coins"))
            {
                GameManager.Instance.AddCoins(coinsToAdd);
                Debug.Log($"💰 Added {coinsToAdd} coins");
            }

            GUILayout.Space(5);

            scoreToadd = EditorGUILayout.IntField("Score to Add", scoreToadd);
            if (GUILayout.Button($"Add {scoreToadd} Score"))
            {
                GameManager.Instance.AddScore(scoreToadd);
                Debug.Log($"🏆 Added {scoreToadd} score");
            }

            GUILayout.Space(5);

            livesToSet = EditorGUILayout.IntField("Set Lives", livesToSet);
            if (GUILayout.Button($"Set Lives to {livesToSet}"))
            {
                SerializedObject so = new SerializedObject(GameManager.Instance);
                so.FindProperty("CurrentLives").intValue = livesToSet;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"❤️ Set lives to {livesToSet}");
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);
        }
        #endregion
    }
}