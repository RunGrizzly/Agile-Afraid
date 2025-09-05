using System.Collections.Generic;
using UnityEngine;
    public class DiscardPile: MonoBehaviour, ILetterDestination
    {
        [SerializeField]
        private List<Letter> m_discardedLetters = new List<Letter>();
        
        public bool IsValid()
        {
            return true;
        }

        public void SendLetter(Letter letter)
        {
            //Remove letter from tile
            m_discardedLetters.Add(letter);
        }
    }
