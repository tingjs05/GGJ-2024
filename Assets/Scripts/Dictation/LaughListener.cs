﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Dictation
{
    public class LaughListener : MonoBehaviour
    {
        public string[] primary_keywords = new string[] {
                "ha",
                "he",
                "ho",
                "hah",
                "har",
                "huh",
                "ah",
                "her",
                "heh",
                "hi",
                "hih",
                "high",
                "laugh",
        };
        

        public bool autoStart = false;
        public event Action OnLaugh;
        public ConfidenceLevel confidence = ConfidenceLevel.Low;
        private KeywordRecognizer recognizer;
        private DictationRecognizer m_DictationRecognizer;
        private string[] GetCapitalVariations(string word)
        {
            int totalVariations = 1 << word.Length;
            List<string> variations = new();

            // Some bit magic to generate all possible variations of capitalization
            for (int i = 0; i < totalVariations; i++)
            {
                string variation = "";
                for (int j = 0; j < word.Length; j++)
                {
                    if (((i >> j) & 1) == 1)
                    {
                        // If the j-th bit is set, convert to uppercase
                        variation += Char.ToUpper(word[j]);
                    }
                    else
                    {
                        // Otherwise, keep it in lowercase
                        variation += Char.ToLower(word[j]);
                    }
                }
                variations.Add(variation);
            }

            return variations.ToArray();
        }
        
        public string[] GetKeywordVariations()
        {
            List<string> keywordVariations = new List<string>();
            List<string> keywords = new List<string>();
            foreach (string keyword in primary_keywords)
            {
                keywords.AddRange(GetCapitalVariations(keyword));
            }
            foreach (string keyword in keywords)
            {
                for (int i = 0; i < 3; i++)
                {
                    keywordVariations.Add(keyword);
                    for (int j = 0; j < i; j++) keywordVariations[keywordVariations.Count-1] += keyword;
                }
                for (int i = 0; i < 3; i++)
                {
                    keywordVariations.Add(keyword);
                    for (int j = 0; j < i; j++) keywordVariations[keywordVariations.Count-1] += " " + keyword;
                }
                
            }
            return keywordVariations.ToArray();
        }

        /// <summary>
        /// Call this to start the listener
        /// </summary>
        public void StartListener()
        {
            DestroyListener();
            Debug.Log("Creating listener with keywords: " + string.Join(", ", GetKeywordVariations()));
            recognizer = new KeywordRecognizer(GetKeywordVariations(), confidence);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
        }

        /// <summary>
        /// Stops the listener
        /// </summary>
        public void DestroyListener()
        {
            if (recognizer != null && recognizer.IsRunning)
            {
                Debug.Log("Destroying listener");
                recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
                recognizer.Stop();
                recognizer.Dispose();
            }
        }

        
        private void Start()
        {
            StartListener();
        }
        
        public void ForceLaugh()
        {
            if (gameObject.activeSelf) OnLaugh?.Invoke();
        }
        
        private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            Debug.Log($"Phrase {args.text} recognized with confidence: {args.confidence}");
            if (gameObject.activeSelf) OnLaugh?.Invoke();
        }

        private void OnDestroy()
        {
            DestroyListener();
            if (m_DictationRecognizer!=null) m_DictationRecognizer.Stop();
        }
    }
}