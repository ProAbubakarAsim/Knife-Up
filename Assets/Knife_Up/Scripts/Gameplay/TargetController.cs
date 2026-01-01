using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetController : MonoBehaviour {

    [Header("Target Config")]
    [SerializeField] private int minRotatingAngle = 50;
    [SerializeField] private int maxRotatingAngle = 360;

    [Header("Target References")]
    [SerializeField] private Rigidbody2D[] brokenPieces;

    private SpriteRenderer spRender = null;
    private CircleCollider2D circleCollider = null;

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
            //Enable all renders
            SetRenderers(true);

            //Start rotating the target
            StartCoroutine(RotatingTarget());
        }   
        else if (obj == GameState.FinishState)
        {
            StartCoroutine(PlayBrokenEffect());
        }
        else if (obj == GameState.GameOver)
        {
            SetRenderers(false);
        }
    }

    private void Awake()
    {
        //Cache components
        spRender = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();      
    }


    private IEnumerator RotatingTarget()
    {
        while (GameManager.Instance.GameState == GameState.Playing)
        {
            Random.InitState(System.Environment.TickCount);
            Vector3 currentAngles = transform.eulerAngles;
            int randomZAngle = Random.Range(minRotatingAngle, maxRotatingAngle);
            Vector3 endAngles = Vector3.zero;
            if (Random.value <= 0.5f)
                endAngles = new Vector3(0, 0, currentAngles.z + randomZAngle);
            else
                endAngles = new Vector3(0, 0, currentAngles.z - randomZAngle);        
            float rotatingTime = randomZAngle / GameManager.Instance.TargetRotatingSpeed;
            LerpType lerpType = GameManager.Instance.ListLerpType[Random.Range(0, GameManager.Instance.ListLerpType.Count)];

            float t = 0;
            while (t < rotatingTime)
            {
                t += Time.deltaTime;
                float factor = EasyType.MatchedLerpType(lerpType, t / rotatingTime);
                transform.eulerAngles = Vector3.Lerp(currentAngles, endAngles, factor);
                yield return null;
            }
        }
    }


    private IEnumerator PlayBrokenEffect()
    {
        yield return new WaitForFixedUpdate();
        spRender.enabled = false;
        circleCollider.enabled = false;

        if (GameManager.Instance.IsBossState) //The current state is boss state
        {
            GameManager.Instance.PlayBossExplodeParticle(transform.position);
        }
        else //The current state isn't boss state
        {
            //Enable breakable pieces
            foreach (Rigidbody2D o in brokenPieces)
            {
                o.gameObject.SetActive(true);
                o.transform.parent = null;

                Vector3 forceDirection = (o.transform.position - transform.position).normalized * 10f;
                forceDirection.y = forceDirection.y < 0 ? forceDirection.y * -1 : forceDirection.y;
                o.AddForceAtPosition(forceDirection, transform.position, ForceMode2D.Impulse);
                o.AddTorque(5, ForceMode2D.Impulse);
                Destroy(o.gameObject, 2f);
                yield return null;
            }
        }      
    }


    /// <summary>
    /// Set renderer of all childs (including the target) 
    /// </summary>
    /// <param name="active"></param>
    public void SetRenderers(bool active)
    {
        SpriteRenderer[] renders = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer o in renders)
        {
            o.enabled = active;
        }
    }


    /// <summary>
    /// Set target sprite by given sprite 
    /// </summary>
    /// <param name="sp"></param>
    public void SetTargetSprite(Sprite sp)
    {
        spRender.sprite = sp;
    }


    /// <summary>
    /// Boucing target up and down
    /// </summary>
    public void BounceTarget()
    {
        StartCoroutine(Bouncing());
    }
    private IEnumerator Bouncing()
    {
        yield return null;
        float t = 0;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + Vector2.up * 0.1f;
        float bouncingTime = 0.05f;
        while (t < bouncingTime)
        {
            t += Time.deltaTime;
            float factor = t / bouncingTime;
            transform.position = Vector2.Lerp(startPos, endPos, factor);
            yield return null;
        }
        t = 0;
        while (t < bouncingTime)
        {
            t += Time.deltaTime;
            float factor = t / bouncingTime;
            transform.position = Vector2.Lerp(endPos, startPos, factor);
            yield return null;
        }
    }
}
