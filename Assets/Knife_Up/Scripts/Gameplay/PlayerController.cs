using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerState
{
    Prepare,
    Living,
    Die,
}

public class PlayerController : MonoBehaviour {

    public static PlayerController Instance { private set; get; }
    public static event System.Action<PlayerState> PlayerStateChanged = delegate { };

    public PlayerState PlayerState
    {
        get
        {
            return playerState;
        }

        private set
        {
            if (value != playerState)
            {
                value = playerState;
                PlayerStateChanged(playerState);
            }
        }
    }


    private PlayerState playerState = PlayerState.Die;


    [Header("Player Config")]
    //[SerializeField]
    //private float playerSpeed = 20f;


    [Header("Player References")]
    [SerializeField]
    private MeshRenderer meshRender;
    // Use this for initialization


    private void OnEnable()
    {
        GameManager.GameStateChanged += GameManager_GameStateChanged;
    }
    private void OnDisable()
    {
        GameManager.GameStateChanged -= GameManager_GameStateChanged;
    }

    private void GameManager_GameStateChanged(GameState obj)
    {
        if (obj == GameState.Playing)
        {
            PlayerLiving();
        }
        else if (obj == GameState.Prepare)
        {
            PlayerPrepare();
        }
    }



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(Instance.gameObject);
            Instance = this;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }



    void Start () {

     
	}
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.GameState==GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                PlayerDie();
                GameManager.Instance.Revive();
            }
        }      
	}

    void PlayerPrepare()
    {
        //Fire event
        PlayerState = PlayerState.Prepare;
        playerState = PlayerState.Prepare;

        //Add another actions here

    }

    void PlayerLiving()
    {
        //Fire event
        PlayerState = PlayerState.Living;
        playerState = PlayerState.Living;

        //Add another actions here

    }

    void PlayerDie()
    {
        //Fire event
        PlayerState = PlayerState.Die;
        playerState = PlayerState.Die;

        //Add another actions here

    }

}
