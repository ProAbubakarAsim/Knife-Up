using UnityEngine;
using System;
using System.Collections;

namespace OnefallGames
{
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager Instance;

        public int Coins
        { 
            get { return currentCoins; }
            private set { currentCoins = value; }
        }

        public static event Action<int> CoinsUpdated = delegate {};

        [SerializeField] int initialCoins = 0;


        // Show the current coins value in editor for easy testing
        [SerializeField] int currentCoins;


        // Key name to store best score in PlayerPrefs
        const string PPK_COINS = "ONEFALL_COINS";


        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void Start()
        {
            ResetCoins();
        }

        public void ResetCoins()
        {
            // Initialize coins
            Coins = PlayerPrefs.GetInt(PPK_COINS, initialCoins);
        }

        public void AddCoins(int amount)
        {
            Coins += amount;


            // Store new coin value
            PlayerPrefs.SetInt(PPK_COINS, Coins);

            // Fire event
            CoinsUpdated(Coins);
        }

        public void RemoveCoins(int amount)
        {
            Coins -= amount;

            // Store new coin value
            PlayerPrefs.SetInt(PPK_COINS, Coins);

            // Fire event
            CoinsUpdated(Coins);
        }
    }
}
