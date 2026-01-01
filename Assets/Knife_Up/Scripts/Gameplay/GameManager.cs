using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OnefallGames;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Serialization;

[System.Serializable]
public struct NormalStateConfig
{
    public int MinState;
    public int MaxState;
    [Range(1, 15)] public int MinKnifeNumber;
    [Range(5, 15)] public int MaxKnifeNumber;
    [Range(20, 300)] public int MinTargetRotatingSpeed;
    [Range(20, 300)] public int MaxTargetRotatingSpeed;
    [Range(0f, 1f)] public float CoinFrequency;
    [Range(1, 16)] public int minCoinNumber;
    [Range(1, 17)] public int maxCoinNumber;
    [Range(0f, 1f)] public float StaticKnifeFrequency;
    [Range(1, 10)] public int minStaticKnifeNumber;
    [Range(1, 10)] public int maxStaticKnifeNumber;
    [Range(0f, 1f)] public float ObstacleFrequency;
    [Range(1, 10)] public int minObstacleNumber;
    [Range(1, 10)] public int maxObstacleNumber;
    public LerpType[] LerpTypes;
}

[System.Serializable]
public struct BossStateConfig
{
    public Sprite BossSprite;
    public string BossName;
    public int FinishedState;
    public int KnifeNumber;
    [Range(100, 400)] public int MinRotatingSpeed;
    [Range(100, 400)] public int MaxRotatingSpeed;
    [Range(0, 17)] public int CoinNumber;
    [Range(0, 10)] public int StaticKnifeNumber;
    [Range(0, 10)] public int ObstacleNumber;
    public LerpType[] LerpTypes;
}


public enum GameState
{
    Prepare,
    Playing,
    Revive,
    GameOver,
    FinishState,
}



public class GameManager : MonoBehaviour {

    public static GameManager Instance { private set; get; }
    public static event System.Action<GameState> GameStateChanged = delegate { };
    public static bool IsRestart { private set; get; }


    private static bool isPlayMusic = false;
    private const string StateSavedPoint_PPK = "STATE_SAVED_POINT";
    private const string BossState_PPK = "BOSS_SAVED_STATE";
    public GameState GameState
    {
        get
        {
            return gameState;
        }
        private set
        {
            if (value != gameState)
            {
                gameState = value;
                GameStateChanged(gameState);
            }
        }
    }

    [Header("Enter a number to test normal state. Set 0 to disable the feature.")]
    [SerializeField] private int testingNormalState = 0;
    [Header("Enter a number to test boss state. Set 0 to disable the feature.")]
    [SerializeField] private int testingBossState = 0;

    [Header("Gameplay Config")]
    [SerializeField] private float knifeDelayTime = 0.05f;
    [SerializeField] private float knifeUpSpeed = 100f;
    [SerializeField] private Color knifeDisableColor;
    [SerializeField] private Color knifeEnableColor;
    public float reviveWaitTime = 4f;
    [SerializeField] private List<int> listSavedState = new List<int>();
    [SerializeField] private List<BossStateConfig> listBossStateConfig = new List<BossStateConfig>();
    [SerializeField] private List<NormalStateConfig> listNormalStateConfig = new List<NormalStateConfig>();


    [Header("Gameplay References")]
    [SerializeField] private TargetController targetControl;
    [SerializeField] private Transform knifeCountUI;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject staticKnifePrefab;
    [SerializeField] private GameObject dynamicKnifePrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject knifeImgPrefab;
    [SerializeField] private GameObject hitTargetParticlePrefab;
    [SerializeField] private GameObject coinExplodeParticlePrefab;
    [SerializeField] private GameObject bossExplodeParticlePrefab;

    public float TargetRotatingSpeed { private set; get; }
    public List<LerpType> ListLerpType { private set; get; }
    public bool IsRevived { private set; get; }
    public bool IsBossState { private set; get; }


    private GameState gameState = GameState.GameOver;
    private List<Vector2> listTargetPos = new List<Vector2>();
    private List<DynamicKnifeController> listDynamicKnifeControl = new List<DynamicKnifeController>();
    private List<ParticleSystem> listHitTargetParticle = new List<ParticleSystem>();
    private List<ParticleSystem> listcoinExplodeParticle = new List<ParticleSystem>();
    private DynamicKnifeController currentDynamicKnifeControl = null;
    private Vector2 knifePosition = Vector2.zero;
    private int knifeCount = 0;
    private bool disableTouch = false;
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

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 60;
        PrepareGame();
	}
	
	// Update is called once per frame
	void Update () {

        if (gameState == GameState.Playing)
        {
            if (Input.GetMouseButtonDown(0) && !disableTouch)
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.throwKnife);
                disableTouch = true;
                knifeCount--;
                currentDynamicKnifeControl.MoveUp(knifeUpSpeed);
                UIManager.Instance.UpdateKnifesImg(knifeCount, knifeDisableColor, knifeEnableColor);
            }
        }
	}


    public void PrepareGame()
    {
        //Fire event
        GameState = GameState.Prepare;
        gameState = GameState.Prepare;

        //Add another actions here

        //Set dynamic knife
        GameObject selectedChar = CharacterManager.Instance.characters[CharacterManager.Instance.SelectedIndex];
        dynamicKnifePrefab.GetComponent<SpriteRenderer>().sprite = selectedChar.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite;


        //Reset isRevived variable
        IsRevived = false;
        IsBossState = false;

        //Calculate knifePosition
        Vector2 bottomPos = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0f));
        knifePosition = bottomPos + Vector2.up * (dynamicKnifePrefab.GetComponent<BoxCollider2D>().size.y / 2f);

        //Get radius
        float targetRadius = targetControl.GetComponent<CircleCollider2D>().radius;
        //Left, right, up, down positions
        listTargetPos.Add((Vector2)targetControl.transform.position + Vector2.up * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + Vector2.down * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + Vector2.left * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + Vector2.right * targetRadius);

        //up_Left, up_Right, down_Left, down_Right
        Vector2 up_Left = (Vector2.up + Vector2.left).normalized;
        Vector2 up_Right = (Vector2.up + Vector2.right).normalized;
        Vector2 down_Left = (Vector2.down + Vector2.left).normalized;
        Vector2 down_Right = (Vector2.down + Vector2.right).normalized;
        listTargetPos.Add((Vector2)targetControl.transform.position + up_Left * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + up_Right * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + down_Left * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + down_Right * targetRadius);

        //left_UpLeft, up_UpLeft, right_UpRight, up_UpRight
        Vector2 left_UpLeft = (Vector2.left + up_Left).normalized;
        Vector2 up_UpLeft = (Vector2.up + up_Left).normalized;
        Vector2 right_UpRight = (Vector2.right + up_Right).normalized;
        Vector2 up_UpRight = (Vector2.up + up_Right).normalized;
        listTargetPos.Add((Vector2)targetControl.transform.position + left_UpLeft * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + up_UpLeft * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + right_UpRight * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + up_UpRight * targetRadius);

        //left_DownLeft, down_DownLeft, right_DownRight, down_DownRight
        Vector2 left_DownLeft = (Vector2.left + down_Left).normalized;
        Vector2 down_DownLeft = (Vector2.down + down_Left).normalized;
        Vector2 right_DownRight = (Vector2.right + down_Right).normalized;
        Vector2 down_DownRight = (Vector2.down + down_Right).normalized;
        listTargetPos.Add((Vector2)targetControl.transform.position + left_DownLeft * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + down_DownLeft * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + right_DownRight * targetRadius);
        listTargetPos.Add((Vector2)targetControl.transform.position + down_DownRight * targetRadius);


        //Set BossSavedState ppk
        if (PlayerPrefs.GetInt(BossState_PPK, 0) == 0)
        {
            PlayerPrefs.SetInt(BossState_PPK, listBossStateConfig[0].FinishedState);
        }

        //Check and creating testing state or boss state or normal state
        HandleState();

        //Wait and enable touch
        disableTouch = true;
        StartCoroutine(EnableTouch(1f));
    }


    /// <summary>
    /// Actual start the game
    /// </summary>
    public void PlayingGame()
    {
        //Fire event
        GameState = GameState.Playing;
        gameState = GameState.Playing;

        //Add another actions here

        //Play background music
        if (!isPlayMusic)
        {
            isPlayMusic = true;
            StartBackgroundMusic(0.5f);
        }

        //Move a dynamic knife to knife position
        MoveKnifeToKnifePosition();
    }


    /// <summary>
    /// Call Revive event
    /// </summary>
    public void Revive()
    {
        //Fire event
        GameState = GameState.Revive;
        gameState = GameState.Revive;
        
        //if (ApplovinManager.instance)
        //{
        //    ApplovinManager.instance.ShowRewardedAd();
        //}

        //Add another actions here
    }


    /// <summary>
    /// Call GameOver event
    /// </summary>
    public void GameOver()
    {
        //Fire event
        GameState = GameState.GameOver;
        gameState = GameState.GameOver;

        //Add another actions here
        StopBackgroundMusic(0.5f);
        IsRestart = true;
        isPlayMusic = false;
        //if (ApplovinManager.instance)
        //{
            
        //    ApplovinManager.instance.ShowRewardedAd();
        //}
        StateManager.Instance.ResetState();
    }

    /// <summary>
    /// Call FinishState event
    /// </summary>
    public void FinishState()
    {
        //Fire event
        GameState = GameState.FinishState;
        gameState = GameState.FinishState;

        //Add another action here
        IsRestart = true;

        if (testingNormalState == 0)
        {
            //Save current state ppk
            if (listSavedState.Contains(StateManager.Instance.CurrentState))
            {
                PlayerPrefs.SetInt(StateSavedPoint_PPK, StateManager.Instance.CurrentState);
            }
        }

        if (IsBossState)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.bossExplode);

            if (testingBossState == 0)
            {
                //Updated boss state ppk
                for (int i = 0; i < listBossStateConfig.Count; i++)
                {
                    if (listBossStateConfig[i].FinishedState > PlayerPrefs.GetInt(BossState_PPK))
                    {
                        PlayerPrefs.SetInt(BossState_PPK, listBossStateConfig[i].FinishedState);
                        break;
                    }
                }
            }
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.finishState);
        }
    }

    public void LoadScene(string sceneName, float delay)
    {
        StartCoroutine(LoadingScene(sceneName, delay));
    }

    private IEnumerator LoadingScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private void StartBackgroundMusic(float delay)
    {
        StartCoroutine(PlayBackgroundMusic(delay));
    }

    private IEnumerator PlayBackgroundMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (SoundManager.Instance.background != null)
            SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
    }

    private void StopBackgroundMusic(float delay)
    {
        StartCoroutine(PauseBackgroundMusic(delay));
    }

    private IEnumerator PauseBackgroundMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (SoundManager.Instance.background != null)
            SoundManager.Instance.StopMusic();
    }

    private ParticleSystem GetHitTargetParticle()
    {
        //Find an inactive particle
        foreach(ParticleSystem o in listHitTargetParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        ParticleSystem par = Instantiate(hitTargetParticlePrefab, Vector2.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        listHitTargetParticle.Add(par);
        par.gameObject.SetActive(false);
        return par;
    }
    private ParticleSystem GetCoinExplodeParticle()
    {
        //Find an inactive particle
        foreach (ParticleSystem o in listcoinExplodeParticle)
        {
            if (!o.gameObject.activeInHierarchy)
                return o;
        }

        //Didn't find one -> create new one
        ParticleSystem par = Instantiate(coinExplodeParticlePrefab, Vector2.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        listcoinExplodeParticle.Add(par);
        par.gameObject.SetActive(false);
        return par;
    }

    //Play particle effect
    private IEnumerator PlayParticle(ParticleSystem par)
    {
        par.Play();
        yield return new WaitForSeconds(par.main.startLifetimeMultiplier);
        par.gameObject.SetActive(false);
    }

    //Wait and enable touch
    private IEnumerator EnableTouch(float delay)
    {
        yield return new WaitForSeconds(delay);
        disableTouch = false;
    }

    //Creating coins for reward effect
    private IEnumerator CreatingCoins(int coinNumber, float delay)
    {
        yield return new WaitForSeconds(delay);

        float leftX = Camera.main.ViewportToWorldPoint(new Vector2(0.4f, 0f)).x;
        float rightX = Camera.main.ViewportToWorldPoint(new Vector2(0.9f, 0f)).x;
        float y = Camera.main.ViewportToWorldPoint(new Vector2(0f, 0f)).y;
        for (int i = 0; i < coinNumber; i++)
        {
            Random.InitState(System.Environment.TickCount);
            float x = Random.Range(leftX, rightX);
            CoinController coinControl = Instantiate(coinPrefab, new Vector2(x, y), Quaternion.identity).GetComponent<CoinController>();
            coinControl.PlayRewardCoinsEffect();
            CoinManager.Instance.AddCoins(1);
            yield return null;
        }
    }

  

    /// <summary>
    /// Create coins, static knives, obstacles
    /// </summary>
    /// <returns></returns>
    private IEnumerator CreateObjects(int coinNumber, int staticKnifeNumber, int obstacleNumber)
    {
        //Creating coins
        for (int i = 0; i < coinNumber; i++)
        {
            //Random an index
            int posIndex = Random.Range(0, listTargetPos.Count);

            Vector2 targetVector = (listTargetPos[posIndex] - (Vector2)targetControl.transform.position).normalized;
            Vector3 angles = new Vector3(0, 0, Vector3.Angle(targetVector, Vector2.up));
            if (listTargetPos[posIndex].x > targetControl.transform.position.x)
                angles = new Vector3(0, 0, 360 - angles.z);
            GameObject coin = Instantiate(coinPrefab, listTargetPos[posIndex], Quaternion.Euler(angles));
            coin.transform.SetParent(targetControl.transform);

            //Remove the position out of the list
            listTargetPos.Remove(listTargetPos[posIndex]);

            yield return null;
        }

        //Creating static knives
        for (int i = 0; i < staticKnifeNumber; i++)
        {
            //Random an index
            int posIndex = Random.Range(0, listTargetPos.Count);

            Vector2 targetVector = (listTargetPos[posIndex] - (Vector2)targetControl.transform.position).normalized;
            Vector3 angles = new Vector3(0, 0, Vector3.Angle(targetVector, Vector2.up));
            if (listTargetPos[posIndex].x > targetControl.transform.position.x)
                angles = new Vector3(0, 0, 360 - angles.z);
            GameObject staticKnife = Instantiate(staticKnifePrefab, listTargetPos[posIndex], Quaternion.Euler(angles));
            staticKnife.transform.SetParent(targetControl.transform);

            //Remove the position out of the list
            listTargetPos.Remove(listTargetPos[posIndex]);

            yield return null;
        }

        //Creating obstacles
        for (int i = 0; i < obstacleNumber; i++)
        {
            //Random an index
            int posIndex = Random.Range(0, listTargetPos.Count);

            Vector2 targetVector = (listTargetPos[posIndex] - (Vector2)targetControl.transform.position).normalized;
            Vector3 angles = new Vector3(0, 0, Vector3.Angle(targetVector, Vector2.up));
            if (listTargetPos[posIndex].x > targetControl.transform.position.x)
                angles = new Vector3(0, 0, 360 - angles.z);
            GameObject obstacle = Instantiate(obstaclePrefab, listTargetPos[posIndex], Quaternion.Euler(angles));
            obstacle.transform.SetParent(targetControl.transform);

            //Remove the position out of the list
            listTargetPos.Remove(listTargetPos[posIndex]);

            yield return null;
        }

        //Disable all renderers (including the target and it's childs)
        targetControl.SetRenderers(false);

        //Actual start the game
        if (IsRestart)
            PlayingGame();
    }


    /// <summary>
    /// Create knife count images base on knifeCount
    /// </summary>
    private void CreateKnifeCountImages()
    {
        //Create knife img
        for (int i = 0; i < knifeCount; i++)
        {
            GameObject knifeImg = Instantiate(knifeImgPrefab, Vector2.zero, Quaternion.Euler(new Vector3(0, 0, 20)));
            knifeImg.transform.SetParent(knifeCountUI);
        }
        //Update knifeImgs
        UIManager.Instance.UpdateKnifesImg(knifeCount, knifeDisableColor, knifeEnableColor);
    }


    /// <summary>
    /// Create state base on given StateConfig
    /// </summary>
    /// <param name="o"></param>
    private void CreateState(NormalStateConfig o)
    {
        //Random knifeCount
        knifeCount = Random.Range(o.MinKnifeNumber, o.MaxKnifeNumber);
        CreateKnifeCountImages();

        //Add lerp type for list
        ListLerpType = new List<LerpType>();
        foreach (LerpType a in o.LerpTypes)
        {
            ListLerpType.Add(a);
        }

        //Random target rotating speed
        TargetRotatingSpeed = Random.Range(o.MinTargetRotatingSpeed, o.MaxTargetRotatingSpeed);

        //Create dynamic knives
        Vector3 knifePos = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, -0.1f));
        for (int i = 0; i < knifeCount; i++)
        {
            GameObject dynamicKnife = Instantiate(dynamicKnifePrefab, Vector2.zero, Quaternion.identity);
            dynamicKnife.transform.position = knifePos;
            listDynamicKnifeControl.Add(dynamicKnife.GetComponent<DynamicKnifeController>());
            dynamicKnife.SetActive(false);
        }

        Random.InitState(System.Environment.TickCount);
        int coinNumber = 0;
        int staticKnifeNumber = 0;
        int obstacleNumber = 0;
        //Random coin number
        if (Random.value <= o.CoinFrequency)
            coinNumber = Random.Range(o.minCoinNumber, o.maxCoinNumber);
        //Random static knife number
        if (Random.value <= o.StaticKnifeFrequency)
            staticKnifeNumber = Random.Range(o.minStaticKnifeNumber, o.maxStaticKnifeNumber);
        //Random obstacle number
        if (Random.value <= o.ObstacleFrequency)
            obstacleNumber = Random.Range(o.minObstacleNumber, o.maxObstacleNumber);

        //Actual creating coins, static knives, obstacles
        StartCoroutine(CreateObjects(coinNumber, staticKnifeNumber, obstacleNumber));
        return;
    }


    /// <summary>
    /// Create boss state base on given BossConfig 
    /// </summary>
    private void CreateBossState(BossStateConfig o)
    {
        IsBossState = true;

        //Set knifeCount, TargetRotatingSpeed, boss sprite, boss's name
        targetControl.SetTargetSprite(o.BossSprite);
        knifeCount = o.KnifeNumber;
        CreateKnifeCountImages();
        UIManager.Instance.SetBossName(o.BossName);
        TargetRotatingSpeed = Random.Range(o.MinRotatingSpeed, o.MaxRotatingSpeed);

        //Set list lerp type
        ListLerpType = new List<LerpType>();
        foreach (LerpType a in o.LerpTypes)
        {
            ListLerpType.Add(a);
        }

        //Create dymanic knives
        Vector3 knifePos = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, -0.1f));
        for (int i = 0; i < knifeCount; i++)
        {
            GameObject dynamicKnife = Instantiate(dynamicKnifePrefab, Vector2.zero, Quaternion.identity);
            dynamicKnife.transform.position = knifePos;
            listDynamicKnifeControl.Add(dynamicKnife.GetComponent<DynamicKnifeController>());
            dynamicKnife.SetActive(false);
        }
 
        //Create coins, static knives, obstacles
        StartCoroutine(CreateObjects(o.CoinNumber, o.StaticKnifeNumber, o.ObstacleNumber));
    }


    /// <summary>
    /// Check and create testing state or boss state or normal state
    /// </summary>
    /// <returns></returns>
    private void HandleState()
    {
        //Check and create normal state for testing
        if (testingNormalState > 0)
        {
            StateManager.Instance.ResetState();
            StateManager.Instance.AddState(testingNormalState);

            //Create normal state for testing
            foreach (NormalStateConfig o in listNormalStateConfig)
            {
                if (StateManager.Instance.CurrentState >= o.MinState && StateManager.Instance.CurrentState < o.MaxState)
                {
                    CreateState(o);
                    break;
                }
            }
        }
        //Check and create boss state for testing
        else if (testingBossState > 0)
        {
            //Create boss state for testing
            foreach (BossStateConfig o in listBossStateConfig)
            {
                if (o.FinishedState == testingBossState)
                {
                    CreateBossState(o);
                    break;
                }
            }
        }
        else //Create normal state and boss state for actual gameplay
        {
            //Determine whether the current state is the boss state  
            if (StateManager.Instance.CurrentState == PlayerPrefs.GetInt(BossState_PPK))
            {
                //Create boss state
                foreach(BossStateConfig o in listBossStateConfig)
                {
                    if (o.FinishedState == PlayerPrefs.GetInt(BossState_PPK))
                    {
                        CreateBossState(o);
                        break;
                    }
                }
            }
            else //Create normal state
            {
                if (StateManager.Instance.CurrentState == 0)
                    StateManager.Instance.AddState(PlayerPrefs.GetInt(StateSavedPoint_PPK, 1));
                else
                    StateManager.Instance.AddState(1);

                //Create the state
                foreach (NormalStateConfig o in listNormalStateConfig)
                {
                    if (StateManager.Instance.CurrentState >= o.MinState && StateManager.Instance.CurrentState < o.MaxState)
                    {
                        CreateState(o);
                        break;
                    }
                }
            }        
        }       
    }


    
    //////////////////////////////////////Publish functions

    
    /// <summary>
    /// Move a dynamic knife to knife position
    /// </summary>
    public void MoveKnifeToKnifePosition()
    {
        currentDynamicKnifeControl = listDynamicKnifeControl[knifeCount - 1];
        currentDynamicKnifeControl.gameObject.SetActive(true);
        currentDynamicKnifeControl.MoveToPosition(knifePosition, knifeDelayTime);
        StartCoroutine(EnableTouch(knifeDelayTime + 0.01f));
    }


    /// <summary>
    /// Play knife hit target particle with given position
    /// </summary>
    /// <param name="pos"></param>
    public void PlayHitTargetParticle(Vector2 pos)
    {
        ParticleSystem par = GetHitTargetParticle();
        par.transform.position = pos;
        par.transform.eulerAngles = new Vector3(180, 0, 0);
        par.gameObject.SetActive(true);
        StartCoroutine(PlayParticle(par));
    }

    /// <summary>
    /// Play coin explode particle with given position
    /// </summary>
    /// <param name="pos"></param>
    public void PlayCoinExplodeParticle(Vector2 pos)
    {
        ParticleSystem par = GetCoinExplodeParticle();
        par.transform.position = pos;
        par.transform.eulerAngles = new Vector3(180, 0, 0);
        par.gameObject.SetActive(true);
        StartCoroutine(PlayParticle(par));
    }


    /// <summary>
    /// Play boss explode particle with given position
    /// </summary>
    /// <param name="pos"></param>
    public void PlayBossExplodeParticle(Vector2 pos)
    {
        ParticleSystem par = Instantiate(bossExplodeParticlePrefab, pos, Quaternion.identity).GetComponent<ParticleSystem>();
        par.transform.position = pos;
        StartCoroutine(PlayParticle(par));
    }


    /// <summary>
    /// Determine whether the knifeCount equals to 0
    /// If knifeCount is 0, user throw all knives
    /// Otherwise , there's still have knives left to throw
    /// </summary>
    /// <returns></returns>
    public bool IsOutOfKnifeCount()
    {
        return knifeCount == 0;
    }


    /// <summary>
    /// Create coins effect for reward
    /// </summary>
    /// <param name="coinNumber"></param>
    public void CreateCoinsForReward(int coinNumber, float delay)
    {
        UIManager.Instance.DisableGameOverUI();
        StartCoroutine(CreatingCoins(coinNumber, delay));
    }


    /// <summary>
    /// Load the saved screenshot
    /// </summary>
    /// <returns></returns>
    public Texture LoadedScrenshot()
    {
        byte[] bytes = File.ReadAllBytes(ShareManager.Instance.ScreenshotPath);
        Texture2D tx = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        tx.LoadImage(bytes);
        return tx;
    }

    /// <summary>
    /// Continue the game
    /// </summary>
    public void SetContinueGame()
    {
        IsRevived = true;
        knifeCount++;
        UIManager.Instance.UpdateKnifesImg(knifeCount, knifeDisableColor, knifeEnableColor);
        PlayingGame();
    }
}
