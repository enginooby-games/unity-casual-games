using UnityEngine;

namespace Devdy.BirthdayCake
{
    /// <summary>
    /// Main game manager that handles game state, config, and restart logic.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public GameConfig Config { get; private set; }
        public GameState CurrentState { get; private set; }
        
        [HideInInspector] public int CurrentLayer;

        [SerializeField] private CakeBuilder cakeBuilder;
        [SerializeField] private UIManager uiManager;

        #region ==================================================================== Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            Config = new GameConfig();
            CurrentState = GameState.Ready;
        }

        private void Start()
        {
            SRDebug.Instance.PinAllOptions("BirthdayCake");
            StartGame();
        }

        #endregion ==================================================================

        #region ==================================================================== Game Flow

        /// <summary>
        /// Starts or restarts the game without reloading the scene.
        /// </summary>
        public void StartGame()
        {
            CurrentState = GameState.Playing;
            CurrentLayer = 0;
            Config.RefreshFromSROptions();
            
            cakeBuilder.Initialize();
            uiManager.ShowGameUI();
        }

        /// <summary>
        /// Restarts the game by resetting all states.
        /// Called when parameters change or player retries.
        /// </summary>
        public void RestartGame()
        {
            cakeBuilder.ResetCake();
            StartGame();
        }

        public void OnCakeComplete()
        {
            CurrentState = GameState.Complete;
            uiManager.ShowVictoryScreen();
            AudioManager.Instance.PlaySound("success");
            // Trigger confetti particle system
            GameObject confetti = GameObject.FindGameObjectWithTag("Confetti");
            if (confetti != null)
            {
                confetti.GetComponent<ParticleSystem>()?.Play();
            }
        }

        public void OnCakeCollapsed()
        {
            CurrentState = GameState.Failed;
            uiManager.ShowFailScreen();
            AudioManager.Instance.PlaySound("fail");
            cakeBuilder.CollapseAllLayers();
        }

        public void OnCakeDropped()
        {
            uiManager.UpdateLayerCount(++CurrentLayer);
        }

        #endregion ==================================================================
    }

    public enum GameState
    {
        Ready,
        Playing,
        Complete,
        Failed
    }
}
