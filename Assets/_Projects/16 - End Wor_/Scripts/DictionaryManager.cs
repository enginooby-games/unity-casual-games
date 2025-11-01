using System.Collections.Generic;
using UnityEngine;

namespace Devdy.EndWor_
{
    /// <summary>
    /// Manages dictionary operations including word validation and prefix checking using a Trie data structure for optimized searching.
    /// </summary>
    public class DictionaryManager : Singleton<DictionaryManager>
    {
        private TrieNode root;
        private HashSet<string> validWords; // Cache for quick word lookup

        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            InitializeDictionary();
        }

        /// <summary>
        /// Initializes the dictionary by loading words and building the Trie structure.
        /// </summary>
        private void InitializeDictionary()
        {
            root = new TrieNode();
            validWords = new HashSet<string>();

            // Load dictionary from Resources
            TextAsset dictionaryFile = Resources.Load<TextAsset>("dictionary");
            
            if (dictionaryFile == null)
            {
                Debug.LogError("Dictionary file not found.");
                return;
            }

            string[] words = dictionaryFile.text.Split('\n');
            foreach (string word in words)
            {
                string cleanWord = word.Trim().ToUpper();
                if (cleanWord.Length >= 3)
                {
                    AddWord(cleanWord);
                }
            }

            Debug.Log($"Dictionary loaded: {validWords.Count} words");
        }

        private void AddWord(string word)
        {
            validWords.Add(word);
            
            TrieNode current = root;
            foreach (char c in word)
            {
                if (!current.Children.ContainsKey(c))
                {
                    current.Children[c] = new TrieNode();
                }
                current = current.Children[c];
            }
            current.IsEndOfWord = true;
        }

        #endregion ==================================================================

        #region Validation

        /// <summary>
        /// Checks if the given string is a valid complete word in the dictionary.
        /// </summary>
        public bool IsValidWord(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 3) return false;
            return validWords.Contains(word.ToUpper());
        }

        /// <summary>
        /// Checks if the given prefix can lead to any valid word in the dictionary.
        /// </summary>
        public bool IsValidPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return true;

            TrieNode current = root;
            foreach (char c in prefix.ToUpper())
            {
                if (!current.Children.ContainsKey(c))
                {
                    return false;
                }
                current = current.Children[c];
            }
            return true;
        }

        /// <summary>
        /// Gets all possible letters that can be appended to the current prefix to form valid words.
        /// </summary>
        public List<char> GetValidNextLetters(string prefix)
        {
            List<char> validLetters = new List<char>();
            
            TrieNode current = root;
            foreach (char c in prefix.ToUpper())
            {
                if (!current.Children.ContainsKey(c))
                {
                    return validLetters; // Prefix doesn't exist
                }
                current = current.Children[c];
            }

            // Get all possible next letters
            foreach (char letter in current.Children.Keys)
            {
                validLetters.Add(letter);
            }

            return validLetters;
        }

        /// <summary>
        /// Checks if appending a letter to the prefix would complete a valid word.
        /// </summary>
        public bool WouldCompleteWord(string prefix, char letter)
        {
            string testWord = prefix + letter;
            return IsValidWord(testWord);
        }

        /// <summary>
        /// Checks if a valid word can be expanded to form other valid words.
        /// For example, "PIN" can be expanded to "PINE", "PINEAPPLE", etc.
        /// </summary>
        public bool CanWordBeExpanded(string word)
        {
            if (string.IsNullOrEmpty(word)) return true;

            // Navigate to the word's node in the trie
            TrieNode current = root;
            foreach (char c in word.ToUpper())
            {
                if (!current.Children.ContainsKey(c))
                {
                    return false; // Word doesn't exist in dictionary
                }
                current = current.Children[c];
            }

            // Check if there are any child nodes (meaning word can be expanded)
            return current.Children.Count > 0;
        }

        #endregion ==================================================================

        #region Trie Node

        private class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; set; }
            public bool IsEndOfWord { get; set; }

            public TrieNode()
            {
                Children = new Dictionary<char, TrieNode>();
                IsEndOfWord = false;
            }
        }

        #endregion ==================================================================
    }
}