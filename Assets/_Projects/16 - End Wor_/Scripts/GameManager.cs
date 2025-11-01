using UnityEngine;
using System.Collections.Generic;

namespace Devdy.EndWor_
{
    /// <summary>
    /// Manages the core game state, turn-based logic, and win condition evaluation.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public enum GameMode
        {
            PlayerVsComputer,
            PlayerVsPlayer
        }

        public enum GameState
        {
            WaitingForPlayer1,
            WaitingForPlayer2,
            ComputerTurn,
            CompletionPhase, // After resignation, remaining player must complete the word
            GameOver
        }

        public enum GameResult
        {
            None,
            Player1Wins,
            Player2Wins,
            Draw
        }

        [SerializeField] private ComputerPlayer computerPlayer; // Reference to AI component

        private GameMode currentMode = GameMode.PlayerVsComputer;
        private GameState currentState;
        private string currentWord = "";
        private GameResult gameResult = GameResult.None;

        private int player1RetryChances = 3; // Retry chances for Player 1
        private int player2RetryChances = 3; // Retry chances for Player 2 (only in PvP mode)
        private int resignedPlayer = 0; // Track which player resigned (1 or 2), 0 = no resignation
        private string wordAtResignation = ""; // Word state when resignation happened
        
        // Track which player added each letter (1 = Player 1, 2 = Player 2/Computer)
        private System.Collections.Generic.List<int> letterOwnership = new System.Collections.Generic.List<int>();

        private const int MAX_RETRY_CHANCES = 3;
        private const int MIN_WORD_LENGTH = 3; // Minimum length for a word to be valid

        #region Initialization

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            currentWord = "";
            currentState = GameState.WaitingForPlayer1;
            gameResult = GameResult.None;
            player1RetryChances = MAX_RETRY_CHANCES;
            player2RetryChances = MAX_RETRY_CHANCES;
            resignedPlayer = 0;
            wordAtResignation = "";
            letterOwnership.Clear();
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
        }

        #endregion ==================================================================

        #region Game Logic

        /// <summary>
        /// Handles player's letter input and validates the move.
        /// </summary>
        public void PlayerAddLetter(char letter)
        {
            if (currentState != GameState.WaitingForPlayer1 && 
                currentState != GameState.WaitingForPlayer2 && 
                currentState != GameState.CompletionPhase) return;

            bool isPlayer1 = currentState == GameState.WaitingForPlayer1 || 
                            (currentState == GameState.CompletionPhase && resignedPlayer == 2);
            letter = char.ToUpper(letter);
            
            // Check if current player has no retry chances left
            if (currentState != GameState.CompletionPhase)
            {
                if (isPlayer1 && player1RetryChances <= 0)
                {
                    UIManager.Instance.ShowPlayerDisqualifiedMessage(1);
                    return; // Player 1 cannot enter any more characters
                }
                else if (!isPlayer1 && player2RetryChances <= 0)
                {
                    UIManager.Instance.ShowPlayerDisqualifiedMessage(2);
                    return; // Player 2 cannot enter any more characters
                }
            }

            // Handle completion phase after resignation
            if (currentState == GameState.CompletionPhase)
            {
                HandleCompletionPhase(letter);
                return;
            }

            // Validate if letter creates valid prefix
            string testWord = currentWord + letter;
            if (!DictionaryManager.Instance.IsValidPrefix(testWord))
            {
                // Invalid move - deduct retry chance
                if (isPlayer1)
                {
                    player1RetryChances--;
                    if (player1RetryChances <= 0)
                    {
                        // No more chances - player 1 is disqualified, enter completion phase
                        UIManager.Instance.ShowNoChancesMessage(1);
                        EnterCompletionPhaseFromDisqualification(1);
                        return;
                    }
                }
                else
                {
                    player2RetryChances--;
                    if (player2RetryChances <= 0)
                    {
                        // No more chances - player 2 is disqualified, enter completion phase
                        UIManager.Instance.ShowNoChancesMessage(2);
                        EnterCompletionPhaseFromDisqualification(2);
                        return;
                    }
                }

                UIManager.Instance.ShowInvalidMoveMessage();
                UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
                return;
            }

            currentWord = testWord;
            
            // Track who added this letter (1 = Player 1, 2 = Player 2/Computer)
            int owner = isPlayer1 ? 1 : 2;
            letterOwnership.Add(owner);
            
            // Check win conditions after player move
            if (CheckWinConditions())
            {
                EndGame();
                return;
            }

            // Switch turn
            SwitchTurn();
        }

        /// <summary>
        /// Enters completion phase when a player is disqualified (no retry chances left).
        /// Similar to resignation - other player must complete the word.
        /// </summary>
        private void EnterCompletionPhaseFromDisqualification(int disqualifiedPlayer)
        {
            resignedPlayer = disqualifiedPlayer;
            wordAtResignation = currentWord;
            
            // If current word is empty, immediate draw
            if (string.IsNullOrEmpty(currentWord))
            {
                gameResult = GameResult.Draw;
                EndGame();
                return;
            }

            // Enter completion phase
            currentState = GameState.CompletionPhase;
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
            UIManager.Instance.ShowDisqualificationMessage(disqualifiedPlayer);
            
            // If in PvC mode and Player 1 was disqualified, computer needs to complete
            if (currentMode == GameMode.PlayerVsComputer && disqualifiedPlayer == 1)
            {
                Invoke(nameof(HandleComputerCompletionPhase), 2f); // Give time for message to show
            }
        }

        /// <summary>
        /// Handles letter input during completion phase after a resignation.
        /// Remaining player must complete the word to win, otherwise it's a draw.
        /// </summary>
        private void HandleCompletionPhase(char letter)
        {
            string testWord = currentWord + letter;
            
            // Check if letter creates valid prefix
            if (!DictionaryManager.Instance.IsValidPrefix(testWord))
            {
                // Invalid move during completion - Draw
                gameResult = GameResult.Draw;
                EndGame();
                return;
            }

            currentWord = testWord;

            // Track ownership (remaining player who is completing the word)
            int owner = resignedPlayer == 1 ? 2 : 1;
            letterOwnership.Add(owner);

            // Check if word is now valid and complete
            if (currentWord.Length >= MIN_WORD_LENGTH && DictionaryManager.Instance.IsValidWord(currentWord))
            {
                // Successfully completed a valid word - remaining player wins
                gameResult = resignedPlayer == 1 ? GameResult.Player2Wins : GameResult.Player1Wins;
                EndGame();
                return;
            }

            // Word still not complete, continue completion phase
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
        }

        /// <summary>
        /// Handles computer AI completing the word during completion phase.
        /// </summary>
        private void HandleComputerCompletionPhase()
        {
            if (currentState != GameState.CompletionPhase) return;

            // Computer needs to complete the word
            List<char> validLetters = DictionaryManager.Instance.GetValidNextLetters(currentWord);
            
            if (validLetters.Count == 0)
            {
                // No valid letters - Draw
                gameResult = GameResult.Draw;
                EndGame();
                return;
            }

            // Find a letter that completes a valid word
            char? completingLetter = null;
            foreach (char letter in validLetters)
            {
                string testWord = currentWord + letter;
                if (DictionaryManager.Instance.IsValidWord(testWord))
                {
                    completingLetter = letter;
                    break;
                }
            }

            // If no letter completes a word, pick a valid letter and continue
            if (!completingLetter.HasValue)
            {
                completingLetter = validLetters[Random.Range(0, validLetters.Count)];
            }

            currentWord += completingLetter.Value;
            letterOwnership.Add(2); // Computer is Player 2

            // Check if word is now valid and complete
            if (currentWord.Length >= MIN_WORD_LENGTH && DictionaryManager.Instance.IsValidWord(currentWord))
            {
                // Computer successfully completed the word
                gameResult = GameResult.Player2Wins;
                EndGame();
                return;
            }

            // Continue completion phase
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
            
            // Computer continues trying to complete the word
            Invoke(nameof(HandleComputerCompletionPhase), 1f);
        }

        /// <summary>
        /// Switches to the next player's turn based on current game mode.
        /// </summary>
        private void SwitchTurn()
        {
            if (currentMode == GameMode.PlayerVsComputer)
            {
                // Switch between Player 1 and Computer
                if (currentState == GameState.WaitingForPlayer1)
                {
                    currentState = GameState.ComputerTurn;
                    UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
                    Invoke(nameof(ComputerTurn), 1f);
                }
                else
                {
                    currentState = GameState.WaitingForPlayer1;
                    UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
                }
            }
            else
            {
                // Player vs Player mode
                if (currentState == GameState.WaitingForPlayer1)
                {
                    // Check if Player 2 has retry chances left
                    if (player2RetryChances <= 0)
                    {
                        // Player 2 has no chances, enter completion phase for Player 1
                        EnterCompletionPhaseFromDisqualification(2);
                        return;
                    }
                    currentState = GameState.WaitingForPlayer2;
                }
                else
                {
                    // Check if Player 1 has retry chances left
                    if (player1RetryChances <= 0)
                    {
                        // Player 1 has no chances, enter completion phase for Player 2
                        EnterCompletionPhaseFromDisqualification(1);
                        return;
                    }
                    currentState = GameState.WaitingForPlayer1;
                }
                UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
            }
        }

        /// <summary>
        /// Executes the computer's turn by choosing and adding a letter.
        /// </summary>
        private void ComputerTurn()
        {
            if (currentState != GameState.ComputerTurn) return;

            char chosenLetter = computerPlayer.ChooseLetter(currentWord);
            string testWord = currentWord + chosenLetter;

            // Validate computer's choice
            if (!DictionaryManager.Instance.IsValidPrefix(testWord))
            {
                // Computer made invalid move - player 1 wins
                gameResult = GameResult.Player1Wins;
                EndGame();
                return;
            }

            currentWord = testWord;

            // Track that computer (Player 2) added this letter
            letterOwnership.Add(2);

            // Check win conditions after computer move
            if (CheckWinConditions())
            {
                EndGame();
                return;
            }

            // Switch back to player 1 turn
            currentState = GameState.WaitingForPlayer1;
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
        }

        /// <summary>
        /// Checks if the current word state triggers any win conditions.
        /// Returns true if game should end.
        /// </summary>
        private bool CheckWinConditions()
        {
            // Win Condition 1: Word is completed AND cannot be expanded further
            if (currentWord.Length >= MIN_WORD_LENGTH && DictionaryManager.Instance.IsValidWord(currentWord))
            {
                // Check if word can be expanded to other valid words
                if (DictionaryManager.Instance.CanWordBeExpanded(currentWord))
                {
                    // Word can be expanded - game continues
                    return false;
                }

                // Word cannot be expanded - current player wins
                if (currentState == GameState.ComputerTurn)
                {
                    gameResult = GameResult.Player2Wins;
                }
                else if (currentState == GameState.WaitingForPlayer1)
                {
                    gameResult = GameResult.Player1Wins;
                }
                else // WaitingForPlayer2
                {
                    gameResult = GameResult.Player2Wins;
                }
                return true;
            }

            // Win Condition 2: No more valid letters can be added
            if (DictionaryManager.Instance.GetValidNextLetters(currentWord).Count == 0)
            {
                // Check if current word is valid
                if (DictionaryManager.Instance.IsValidWord(currentWord))
                {
                    // Current player wins (completed word)
                    if (currentState == GameState.ComputerTurn)
                    {
                        gameResult = GameResult.Player2Wins;
                    }
                    else if (currentState == GameState.WaitingForPlayer1)
                    {
                        gameResult = GameResult.Player1Wins;
                    }
                    else // WaitingForPlayer2
                    {
                        gameResult = GameResult.Player2Wins;
                    }
                }
                else
                {
                    // Other player wins (current word is invalid and stuck)
                    if (currentState == GameState.ComputerTurn)
                    {
                        gameResult = GameResult.Player1Wins;
                    }
                    else if (currentState == GameState.WaitingForPlayer1)
                    {
                        gameResult = GameResult.Player2Wins;
                    }
                    else // WaitingForPlayer2
                    {
                        gameResult = GameResult.Player1Wins;
                    }
                }
                return true;
            }

            return false;
        }

        private void EndGame()
        {
            currentState = GameState.GameOver;
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
        }

        public void RestartGame()
        {
            CancelInvoke(); // Cancel any pending computer turns
            InitializeGame();
        }

        /// <summary>
        /// Player resigns from the current game.
        /// The remaining player must complete the current word to win, otherwise it's a draw.
        /// </summary>
        public void PlayerResign()
        {
            if (currentState == GameState.GameOver || currentState == GameState.CompletionPhase) return;
            
            CancelInvoke(); // Cancel any pending computer turns
            
            // Determine who resigned
            if (currentState == GameState.WaitingForPlayer1)
            {
                resignedPlayer = 1; // Player 1 resigned, Player 2 must complete
            }
            else if (currentState == GameState.WaitingForPlayer2)
            {
                resignedPlayer = 2; // Player 2 resigned, Player 1 must complete
            }
            else // ComputerTurn
            {
                resignedPlayer = 2; // Computer resigned, Player 1 must complete
            }

            wordAtResignation = currentWord;
            
            // If current word is empty, immediate draw (nothing to complete)
            if (string.IsNullOrEmpty(currentWord))
            {
                gameResult = GameResult.Draw;
                EndGame();
                return;
            }

            // Enter completion phase
            currentState = GameState.CompletionPhase;
            UIManager.Instance.UpdateUI(currentWord, currentState, gameResult, player1RetryChances, player2RetryChances, currentMode, letterOwnership);
            UIManager.Instance.ShowCompletionPhaseMessage(resignedPlayer);
            
            // If in PvC mode and Player 1 resigned, computer needs to complete
            if (currentMode == GameMode.PlayerVsComputer && resignedPlayer == 1)
            {
                Invoke(nameof(HandleComputerCompletionPhase), 2f); // Give time for message to show
            }
        }

        /// <summary>
        /// Sets the game mode (Player vs Computer or Player vs Player).
        /// </summary>
        public void SetGameMode(GameMode mode)
        {
            currentMode = mode;
            RestartGame();
        }

        #endregion ==================================================================

        #region Getters

        public string GetCurrentWord()
        {
            return currentWord;
        }

        public GameState GetCurrentState()
        {
            return currentState;
        }

        public GameResult GetGameResult()
        {
            return gameResult;
        }

        public GameMode GetCurrentMode()
        {
            return currentMode;
        }

        public int GetPlayer1RetryChances()
        {
            return player1RetryChances;
        }

        public int GetPlayer2RetryChances()
        {
            return player2RetryChances;
        }

        public ComputerPlayer GetComputerPlayer()
        {
            return computerPlayer;
        }

        public System.Collections.Generic.List<int> GetLetterOwnership()
        {
            return letterOwnership;
        }

        #endregion ==================================================================
    }
}