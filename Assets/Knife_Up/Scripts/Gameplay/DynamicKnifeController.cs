using System.Collections;
using OnefallGames;
using UnityEngine;

public class DynamicKnifeController : MonoBehaviour {

    private TargetController targetControl = null;
    private Rigidbody2D rigid = null;
    private BoxCollider2D boxCollider = null;

    private Vector2 originalPos = Vector2.zero;
    private float normalKnifeForceUp = 8f;
    private float lastKnifeForceUp = 25f;
    private float lastKnideTorque = 15f;
    private float normalKnifeTorque = 7f;

    private float forceUp = 0;
    private float torque = 0;
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
        if (obj == GameState.FinishState)
        {
            transform.parent = null;
            boxCollider.enabled = false;
            Vector2 forceDir = (forceUp == lastKnifeForceUp) ? Vector2.up : 
                (Vector2)(transform.position - targetControl.transform.position).normalized;
            forceDir *= forceUp;
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.AddForceAtPosition(forceDir, targetControl.transform.position, ForceMode2D.Impulse);
            rigid.AddTorque(torque, ForceMode2D.Impulse);
            Destroy(gameObject, 2f);
        }
    }

    void Start () {

        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        targetControl = FindObjectOfType<TargetController>();
	}

    public void MoveUp(float speed)
    {
        RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position + Vector2.left * (boxCollider.size.x / 2f), Vector2.up, 10f);
        if (!hit.collider.CompareTag("Finish"))
            hit = Physics2D.Raycast((Vector2)transform.position + Vector2.right * (boxCollider.size.x / 2f), Vector2.up, 10f);
        if (!hit.collider.CompareTag("Finish"))
            hit = Physics2D.Raycast(transform.position, Vector2.up, 10f);
        StartCoroutine(MovingUp(speed, hit));
    }
    private IEnumerator MovingUp(float speed, RaycastHit2D hit)
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = hit.point + Vector2.down * (boxCollider.size.y / 2f);
       
        float movingTime = Vector3.Distance(transform.position, endPos) / speed;
        float t = 0;
        while (t < movingTime)
        {
            t += Time.deltaTime;
            float factor = t / movingTime;
            transform.position = Vector3.Lerp(startPos, endPos, factor);
            yield return null;
        }

        if (hit.collider.CompareTag("Finish")) //Hit other knives
        {
            //Capture screenshot
            ShareManager.Instance.CreateScreenshot();

            //Play sound effect
            SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);

            boxCollider.enabled = false;
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.AddTorque(10, ForceMode2D.Impulse);
            StartCoroutine(DisableObject());
        }
        else //hit the target
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.hitTarget);

            //Set knife position
            Vector2 pos = (Vector2)hit.transform.position
                + Vector2.down * hit.collider.GetComponent<CircleCollider2D>().radius;
            transform.position = pos;

            //Set parent for this knife 
            transform.SetParent(hit.transform);

            //Bouncing the target
            targetControl.BounceTarget();

            //Play knife hit target particle
            GameManager.Instance.PlayHitTargetParticle(pos);

            if (!GameManager.Instance.IsOutOfKnifeCount()) //Still have knife left
            {
                //Move another dynamic knife to knife's position 
                GameManager.Instance.MoveKnifeToKnifePosition();

                forceUp = normalKnifeForceUp;
                torque = normalKnifeTorque;
            }
            else //User throw all knifes
            {
                forceUp = lastKnifeForceUp;
                torque = lastKnideTorque;

                GameManager.Instance.FinishState();
            }
        }
    }

    IEnumerator DisableObject()
    {
        while (gameObject.activeInHierarchy)
        {
            Vector2 viewPortPos = Camera.main.WorldToViewportPoint(transform.position);
            if (viewPortPos.x >= 1.1f || viewPortPos.x <= -0.1f || viewPortPos.y <= -0.1f || viewPortPos.y >= 1.1f)
            {
                rigid.bodyType = RigidbodyType2D.Static;
                transform.position = originalPos;
                transform.eulerAngles = Vector3.zero;
                boxCollider.enabled = true;
                gameObject.SetActive(false);

                if (GameManager.Instance.IsRevived)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {

                    if (GameManager.Instance)
                    {
                        GameManager.Instance.Revive();
                        
                    }
                    else
                    {
                        GameManager.Instance.GameOver();
                    }
                }
               
                yield break;
            }
            yield return null;
        }
    }




    public void MoveToPosition(Vector2 pos, float time)
    {
        if (originalPos == Vector2.zero)
            originalPos = transform.position;
        StartCoroutine(MovingToPos(pos, time));
    }
    private IEnumerator MovingToPos(Vector2 pos, float time)
    {
        float t = 0;
        Vector2 startPos = transform.position;
        while (t < time)
        {
            t += Time.deltaTime;
            float factor = t / time;
            transform.position = Vector2.Lerp(startPos, pos, factor);
            yield return null;
        }
    }

}
