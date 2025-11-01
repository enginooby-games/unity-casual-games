using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Devdy.EndWor_
{
    /// <summary>
    /// Handles computer AI logic for choosing letters based on difficulty mode.
    /// </summary>
    public class ComputerPlayer : MonoBehaviour
    {
        public enum Difficulty
        {
            Easy,
            Hard
        }

        private Difficulty currentDifficulty = Difficulty.Easy;

        /// <summary>
        /// Sets the AI difficulty mode.
        /// </summary>
        public void SetDifficulty(Difficulty difficulty)
        {
            currentDifficulty = difficulty;
            Debug.Log($"AI Difficulty set to: {difficulty}");
        }

        /// <summary>
        /// Chooses the next letter based on current difficulty mode.
        /// </summary>
        public char ChooseLetter(string currentWord)
        {
            return currentDifficulty == Difficulty.Easy 
                ? ChooseRandomLetter(currentWord) 
                : ChooseStrategicLetter(currentWord);
        }

        /// <summary>
        /// Easy mode: randomly selects a valid letter that can be appended.
        /// </summary>
        private char ChooseRandomLetter(string currentWord)
        {
            List<char> validLetters = DictionaryManager.Instance.GetValidNextLetters(currentWord);
            
            if (validLetters.Count == 0)
            {
                // Fallback: return a random letter A-Z
                return (char)Random.Range('A', 'Z' + 1);
            }

            return validLetters[Random.Range(0, validLetters.Count)];
        }

        /// <summary>
        /// Hard mode: strategically chooses a letter to maximize winning chances.
        /// Prioritizes letters that don't complete words and keep more future options open.
        /// </summary>
        private char ChooseStrategicLetter(string currentWord)
        {
            List<char> validLetters = DictionaryManager.Instance.GetValidNextLetters(currentWord);
            
            if (validLetters.Count == 0)
            {
                return (char)Random.Range('A', 'Z' + 1);
            }

            // Strategy 1: Avoid completing words
            List<char> safeLetters = validLetters
                .Where(letter => !DictionaryManager.Instance.WouldCompleteWord(currentWord, letter))
                .ToList();

            if (safeLetters.Count > 0)
            {
                // Strategy 2: Choose letter that keeps most future options open
                char bestLetter = safeLetters[0];
                int maxOptions = 0;

                foreach (char letter in safeLetters)
                {
                    string testWord = currentWord + letter;
                    int optionCount = DictionaryManager.Instance.GetValidNextLetters(testWord).Count;
                    
                    if (optionCount > maxOptions)
                    {
                        maxOptions = optionCount;
                        bestLetter = letter;
                    }
                }

                return bestLetter;
            }

            // All letters complete words - choose one that gives opponent fewer options
            char leastBadLetter = validLetters[0];
            int minOpponentOptions = int.MaxValue;

            foreach (char letter in validLetters)
            {
                string testWord = currentWord + letter;
                int opponentOptions = DictionaryManager.Instance.GetValidNextLetters(testWord).Count;
                
                if (opponentOptions < minOpponentOptions)
                {
                    minOpponentOptions = opponentOptions;
                    leastBadLetter = letter;
                }
            }

            return leastBadLetter;
        }

        public Difficulty GetCurrentDifficulty()
        {
            return currentDifficulty;
        }
    }
}
