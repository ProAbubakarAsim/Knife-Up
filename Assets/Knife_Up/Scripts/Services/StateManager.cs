using UnityEngine;
using System;
using System.Collections;

namespace OnefallGames
{
    public class StateManager : MonoBehaviour
    {
        public static StateManager Instance { get; private set; }

        public int CurrentState { get; private set; }

        public int HighestState { get; private set; }

        public bool HasNewBestState { get; private set; }

        public static event Action<int> StateUpdated = delegate {};
        public static event Action<int> HighestStateUpdated = delegate {};

        private const string HIGHESTSTATE = "HIGHESTSTATE";
        // key name to store high score in PlayerPrefs

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
            ResetState();
        }

        public void ResetState()
        {
            // Initialize state
            CurrentState = 0;

            // Initialize highest state
            HighestState = PlayerPrefs.GetInt(HIGHESTSTATE, 0);
            HasNewBestState = false;
        }

        public void AddState(int amount)
        {
            CurrentState += amount;

            // Fire event
            StateUpdated(CurrentState);

            if (CurrentState > HighestState)
            {
                UpdateHighState(CurrentState);
                HasNewBestState = true;
            }
            else
            {
                HasNewBestState = false;
            }
        }

        public void UpdateHighState(int newHighState)
        {
            // Update highstate if player has made a new one
            if (newHighState > HighestState)
            {
                HighestState = newHighState;
                PlayerPrefs.SetInt(HIGHESTSTATE, HighestState);
                HighestStateUpdated(HighestState);
            }
        }
    }
}
