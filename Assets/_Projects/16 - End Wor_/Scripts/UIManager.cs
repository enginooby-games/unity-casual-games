using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Devdy.EndWor_
{
    /// <summary>
    /// Manages all UI elements including word display, turn indicator, game results, and AI difficulty toggle.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Word Display")]
        [SerializeField] private TextMeshProUGUI currentWordText; // Displays the current word being built
        [SerializeField] private TextMeshProUGUI turnIndicatorText; // Shows whose turn it is
        [SerializeField] private TextMeshProUGUI retryChancesText; // Shows remaining retry chances

        [Header("Game Result")]
        [SerializeField] private GameObject gameOverPanel; // Panel shown when game ends
        [SerializeField] private TextMeshProUGUI gameResultText; // Displays win/lose/draw message
        [SerializeField] private Button restartButton; // Button to restart the game

        [Header("Player Input")]
        [SerializeField] private TMP_InputField letterInputField; // Input field for player to enter letter
        [SerializeField] private Button submitLetterButton; // Button to submit the letter
        [SerializeField] private TextMeshProUGUI invalidMoveText; // Shows error message for invalid moves
        [SerializeField] private Button resignButton; // Button for player to resign

        [Header("AI Settings")]
        [SerializeField] private Button aiModeToggleButton; // Button to switch AI difficulty
        [SerializeField] private TextMeshProUGUI aiModeText; // Displays current AI mode (Easy/Hard)

        [Header("Game Mode")]
        [SerializeField] private Button gameModeToggleButton; // Button to switch between PvC and PvP
        [SerializeField] private TextMeshProUGUI gameModeText; // Displays current game mode

        private float invalidMessageDuration = 2f; // Duration to show invalid move message

        #region Initialization

        private void Start()
        {
            SetupButtons();
            HideInvalidMoveMessage();
            UpdateAIModeText();
            UpdateGameModeText();
        }

        private void SetupButtons()
        {
            if (submitLetterButton != null)
            {
                submitLetterButton.onClick.AddListener(OnSubmitLetterClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (aiModeToggleButton != null)
            {
                aiModeToggleButton.onClick.AddListener(OnAIModeToggleClicked);
            }

            if (gameModeToggleButton != null)
            {
                gameModeToggleButton.onClick.AddListener(OnGameModeToggleClicked);
            }

            if (resignButton != null)
            {
                resignButton.onClick.AddListener(OnResignClicked);
            }

            if (letterInputField != null)
            {
                letterInputField.characterLimit = 1;
                letterInputField.onSubmit.AddListener(OnInputFieldSubmit);
            }
        }

        #endregion ==================================================================

        #region UI Update

        /// <summary>
        /// Updates all UI elements based on current game state.
        /// </summary>
        public void UpdateUI(string currentWord, GameManager.GameState gameState, GameManager.GameResult gameResult, 
            int player1Chances, int player2Chances, GameManager.GameMode gameMode, System.Collections.Generic.List<int> letterOwnership)
        {
            UpdateWordDisplay(currentWord, letterOwnership, gameMode);
            UpdateTurnIndicator(gameState, gameMode);
            UpdateRetryChances(player1Chances, player2Chances, gameState, gameMode);
            UpdateGameResult(gameState, gameResult, gameMode);
            UpdateInputInteractivity(gameState);
            UpdateAIModeVisibility(gameMode);
        }

        private void UpdateWordDisplay(string word, System.Collections.Generic.List<int> letterOwnership, GameManager.GameMode gameMode)
        {
            if (currentWordText == null) return;

            if (string.IsNullOrEmpty(word))
            {
                currentWordText.text = "___";
                return;
            }

            // Build colored text using TextMeshPro rich text tags
            string coloredWord = "";
            for (int i = 0; i < word.Length; i++)
            {
                char letter = word[i];
                
                // Get owner of this letter (default to 1 if ownership data missing)
                int owner = (i < letterOwnership.Count) ? letterOwnership[i] : 1;
                
                // Determine color based on owner
                string color = GetPlayerColor(owner, gameMode);
                
                // Add letter with color tag
                coloredWord += $"<color={color}>{letter}</color>";
            }

            currentWordText.text = coloredWord;
        }

        /// <summary>
        /// Returns the hex color for a given player.
        /// </summary>
        private string GetPlayerColor(int playerNumber, GameManager.GameMode gameMode)
        {
            if (playerNumber == 1)
            {
                // Player 1: Green
                return "#4CAF50";
            }
            else
            {
                // Player 2 or Computer: Cyan
                return "#00BCD4";
            }
        }

        private void UpdateTurnIndicator(GameManager.GameState gameState, GameManager.GameMode gameMode)
        {
            if (turnIndicatorText == null) return;

            switch (gameState)
            {
                case GameManager.GameState.WaitingForPlayer1:
                    turnIndicatorText.text = "Player 1's Turn";
                    turnIndicatorText.color = Color.green;
                    break;
                case GameManager.GameState.WaitingForPlayer2:
                    string player2Name = gameMode == GameManager.GameMode.PlayerVsComputer ? "Computer" : "Player 2";
                    turnIndicatorText.text = $"{player2Name}'s Turn";
                    turnIndicatorText.color = Color.cyan;
                    break;
                case GameManager.GameState.ComputerTurn:
                    turnIndicatorText.text = "Computer's Turn";
                    turnIndicatorText.color = Color.cyan;
                    break;
                case GameManager.GameState.CompletionPhase:
                    turnIndicatorText.text = "Completion Phase";
                    turnIndicatorText.color = Color.yellow;
                    break;
                case GameManager.GameState.GameOver:
                    turnIndicatorText.text = "Game Over";
                    turnIndicatorText.color = Color.red;
                    break;
            }
        }

        private void UpdateRetryChances(int player1Chances, int player2Chances, GameManager.GameState gameState, GameManager.GameMode gameMode)
        {
            if (retryChancesText == null) return;

            if (gameState == GameManager.GameState.WaitingForPlayer1)
            {
                retryChancesText.text = $"Retry Chances: {player1Chances}/3";
                retryChancesText.color = player1Chances > 1 ? Color.white : (player1Chances == 1 ? Color.yellow : Color.red);
            }
            else if (gameState == GameManager.GameState.WaitingForPlayer2)
            {
                retryChancesText.text = $"Retry Chances: {player2Chances}/3";
                retryChancesText.color = player2Chances > 1 ? Color.white : (player2Chances == 1 ? Color.yellow : Color.red);
            }
            else
            {
                retryChancesText.text = "";
            }
        }

        private void UpdateGameResult(GameManager.GameState gameState, GameManager.GameResult gameResult, GameManager.GameMode gameMode)
        {
            if (gameOverPanel == null || gameResultText == null) return;

            if (gameState == GameManager.GameState.GameOver)
            {
                gameOverPanel.SetActive(true);
                
                switch (gameResult)
                {
                    case GameManager.GameResult.Player1Wins:
                        gameResultText.text = "Player 1 Wins!";
                        gameResultText.color = Color.green;
                        break;
                    case GameManager.GameResult.Player2Wins:
                        string player2Name = gameMode == GameManager.GameMode.PlayerVsComputer ? "Computer" : "Player 2";
                        gameResultText.text = $"{player2Name} Wins!";
                        gameResultText.color = gameMode == GameManager.GameMode.PlayerVsComputer ? Color.red : Color.cyan;
                        break;
                    case GameManager.GameResult.Draw:
                        gameResultText.text = "Draw!";
                        gameResultText.color = Color.yellow;
                        break;
                }
            }
            else
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void UpdateInputInteractivity(GameManager.GameState gameState)
        {
            bool isPlayerTurn = gameState == GameManager.GameState.WaitingForPlayer1 || 
                               gameState == GameManager.GameState.WaitingForPlayer2 ||
                               gameState == GameManager.GameState.CompletionPhase;
            bool isGameActive = gameState != GameManager.GameState.GameOver;
            
            // Check if current player has retry chances
            bool canCurrentPlayerInput = true;
            if (gameState == GameManager.GameState.WaitingForPlayer1)
            {
                canCurrentPlayerInput = GameManager.Instance.GetPlayer1RetryChances() > 0;
            }
            else if (gameState == GameManager.GameState.WaitingForPlayer2)
            {
                canCurrentPlayerInput = GameManager.Instance.GetPlayer2RetryChances() > 0;
            }
            
            if (letterInputField != null)
            {
                letterInputField.interactable = isPlayerTurn && canCurrentPlayerInput;
                if (isPlayerTurn && canCurrentPlayerInput)
                {
                    letterInputField.text = "";
                    letterInputField.ActivateInputField();
                }
            }

            if (submitLetterButton != null)
            {
                submitLetterButton.interactable = isPlayerTurn && canCurrentPlayerInput;
            }

            if (resignButton != null)
            {
                // Disable resign button during completion phase or if player has no chances
                resignButton.interactable = isGameActive && 
                                           gameState != GameManager.GameState.CompletionPhase && 
                                           canCurrentPlayerInput;
            }
        }

        private void UpdateAIModeVisibility(GameManager.GameMode gameMode)
        {
            // Hide AI mode toggle in Player vs Player mode
            bool showAIMode = gameMode == GameManager.GameMode.PlayerVsComputer;
            
            if (aiModeToggleButton != null)
            {
                aiModeToggleButton.gameObject.SetActive(showAIMode);
            }
            
            if (aiModeText != null)
            {
                aiModeText.gameObject.SetActive(showAIMode);
            }
        }

        #endregion ==================================================================

        #region Button Handlers

        private void OnSubmitLetterClicked()
        {
            if (letterInputField == null) return;

            string input = letterInputField.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(input)) return;

            char letter = input[0];
            GameManager.Instance.PlayerAddLetter(letter);
            letterInputField.text = "";
            letterInputField.ActivateInputField();
        }

        private void OnInputFieldSubmit(string input)
        {
            OnSubmitLetterClicked();
        }

        private void OnRestartClicked()
        {
            GameManager.Instance.RestartGame();
        }

        private void OnResignClicked()
        {
            GameManager.Instance.PlayerResign();
        }

        private void OnAIModeToggleClicked()
        {
            ComputerPlayer computerPlayer = GameManager.Instance.GetComputerPlayer();
            if (computerPlayer == null) return;

            // Toggle between Easy and Hard
            ComputerPlayer.Difficulty currentDifficulty = computerPlayer.GetCurrentDifficulty();
            ComputerPlayer.Difficulty newDifficulty = currentDifficulty == ComputerPlayer.Difficulty.Easy 
                ? ComputerPlayer.Difficulty.Hard 
                : ComputerPlayer.Difficulty.Easy;

            computerPlayer.SetDifficulty(newDifficulty);
            UpdateAIModeText();
        }

        private void OnGameModeToggleClicked()
        {
            GameManager.GameMode currentMode = GameManager.Instance.GetCurrentMode();
            GameManager.GameMode newMode = currentMode == GameManager.GameMode.PlayerVsComputer 
                ? GameManager.GameMode.PlayerVsPlayer 
                : GameManager.GameMode.PlayerVsComputer;

            GameManager.Instance.SetGameMode(newMode);
            UpdateGameModeText();
        }

        private void UpdateAIModeText()
        {
            if (aiModeText == null) return;

            ComputerPlayer computerPlayer = GameManager.Instance.GetComputerPlayer();
            if (computerPlayer == null) return;

            ComputerPlayer.Difficulty currentDifficulty = computerPlayer.GetCurrentDifficulty();
            aiModeText.text = $"AI: {currentDifficulty}";
        }

        private void UpdateGameModeText()
        {
            if (gameModeText == null) return;

            GameManager.GameMode currentMode = GameManager.Instance.GetCurrentMode();
            gameModeText.text = currentMode == GameManager.GameMode.PlayerVsComputer 
                ? "Player vs. AI" 
                : "Player vs. Player";
        }

        #endregion ==================================================================

        #region Messages

        public void ShowInvalidMoveMessage()
        {
            if (invalidMoveText == null) return;
            
            invalidMoveText.gameObject.SetActive(true);
            invalidMoveText.text = "Invalid move! Letter doesn't form a valid prefix.";
            invalidMoveText.color = Color.red;
            
            CancelInvoke(nameof(HideInvalidMoveMessage));
            Invoke(nameof(HideInvalidMoveMessage), invalidMessageDuration);
        }

        public void ShowNoChancesMessage(int playerNumber)
        {
            if (invalidMoveText == null) return;
            
            invalidMoveText.gameObject.SetActive(true);
            invalidMoveText.text = $"Player {playerNumber} has no retry chances left! Turn skipped.";
            invalidMoveText.color = Color.yellow;
            
            CancelInvoke(nameof(HideInvalidMoveMessage));
            Invoke(nameof(HideInvalidMoveMessage), invalidMessageDuration + 1f);
        }

        public void ShowCompletionPhaseMessage(int resignedPlayer)
        {
            if (invalidMoveText == null) return;
            
            invalidMoveText.gameObject.SetActive(true);
            string remainingPlayer = resignedPlayer == 1 ? "Player 2" : "Player 1";
            invalidMoveText.text = $"Player {resignedPlayer} resigned! {remainingPlayer} must complete the word to win!";
            invalidMoveText.color = Color.yellow;
            
            CancelInvoke(nameof(HideInvalidMoveMessage));
            Invoke(nameof(HideInvalidMoveMessage), invalidMessageDuration + 2f);
        }

        public void ShowPlayerDisqualifiedMessage(int playerNumber)
        {
            if (invalidMoveText == null) return;
            
            invalidMoveText.gameObject.SetActive(true);
            invalidMoveText.text = $"Player {playerNumber} has no retry chances! Cannot enter characters.";
            invalidMoveText.color = Color.red;
            
            CancelInvoke(nameof(HideInvalidMoveMessage));
            Invoke(nameof(HideInvalidMoveMessage), invalidMessageDuration);
        }

        public void ShowDisqualificationMessage(int disqualifiedPlayer)
        {
            if (invalidMoveText == null) return;
            
            invalidMoveText.gameObject.SetActive(true);
            string remainingPlayer = disqualifiedPlayer == 1 ? "Player 2" : "Player 1";
            invalidMoveText.text = $"Player {disqualifiedPlayer} disqualified! {remainingPlayer} must complete the word to win!";
            invalidMoveText.color = new Color(1f, 0.5f, 0f); // Orange
            
            CancelInvoke(nameof(HideInvalidMoveMessage));
            Invoke(nameof(HideInvalidMoveMessage), invalidMessageDuration + 2f);
        }

        private void HideInvalidMoveMessage()
        {
            if (invalidMoveText != null)
            {
                invalidMoveText.gameObject.SetActive(false);
            }
        }

        #endregion ==================================================================
    }
}