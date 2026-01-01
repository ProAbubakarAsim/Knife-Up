using System.Collections;
using OnefallGames;
using UnityEngine;

public class CoinController : MonoBehaviour {

    [SerializeField] private Rigidbody2D[] coinPieces;

    private Rigidbody2D rigid = null;
    private SpriteRenderer spRender = null;
    private BoxCollider2D boxCollider = null;
    private Transform target = null;

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

            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.AddForceAtPosition(Vector2.up * 10f, target.position, ForceMode2D.Impulse);

            rigid.AddTorque(3f, ForceMode2D.Impulse);
        }
    }

    void Start () {

        rigid = GetComponent<Rigidbody2D>();
        spRender = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        target = transform.parent;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameManager.Instance.GameState == GameState.Playing)
        {
            if (collision.CompareTag("Finish"))
            {
                SoundManager.Instance.PlaySound(SoundManager.Instance.hitCoin);
                CoinManager.Instance.AddCoins(1);

                transform.SetParent(null);

                //Disable parent render and collider  
                spRender.enabled = false;
                boxCollider.enabled = false;

                //Force the pieces down
                foreach (Rigidbody2D o in coinPieces)
                {
                    o.gameObject.SetActive(true);
                    o.transform.SetParent(null);

                    Vector2 dir = (o.transform.position - transform.position).normalized * 10f;
                    o.AddForceAtPosition(dir, transform.position, ForceMode2D.Impulse);
                    o.AddTorque(5f, ForceMode2D.Impulse);
                    Destroy(o.gameObject, 2f);
                }

                Destroy(gameObject);
            }
        }
    }


    /// <summary>
    /// Play reward coins effect (only for reward coins)
    /// </summary>
    public void PlayRewardCoinsEffect()
    {
        if (rigid == null)
            rigid = GetComponent<Rigidbody2D>();
        if (spRender == null)
            spRender = GetComponent<SpriteRenderer>();
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider2D>();

        transform.SetParent(null);

        rigid.bodyType = RigidbodyType2D.Dynamic;
        float upForce = Random.Range(20f, 40f);
        rigid.AddForce(Vector2.up * upForce, ForceMode2D.Impulse);

        rigid.AddTorque(3f, ForceMode2D.Impulse);

        StartCoroutine(RewardingCoinsEffect());
    }
    IEnumerator RewardingCoinsEffect()
    {
        while (rigid.velocity.y > 0)
        {
            yield return null;
        }

        SoundManager.Instance.PlaySound(SoundManager.Instance.hitCoin);
        spRender.enabled = false;
        boxCollider.enabled = false;

        GameManager.Instance.PlayCoinExplodeParticle(transform.position);

        foreach (Rigidbody2D o in coinPieces)
        {
            o.gameObject.SetActive(true);
            o.transform.SetParent(null);

            Vector2 dir = (o.transform.position - transform.position).normalized * 8f;
            o.AddForceAtPosition(dir, transform.position, ForceMode2D.Impulse);
            o.AddTorque(4f, ForceMode2D.Impulse);
            Destroy(o.gameObject, 2f);
        }

        Destroy(gameObject);
    }


}
