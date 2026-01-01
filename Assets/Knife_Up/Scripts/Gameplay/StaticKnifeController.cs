using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticKnifeController : MonoBehaviour {

    private Transform target = null;
    private Rigidbody2D rigid = null;
    private BoxCollider2D boxCollider = null;

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
            Vector2 forceDir = (transform.position - target.position).normalized * 8f;
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.AddForceAtPosition(forceDir, target.position, ForceMode2D.Impulse);
            rigid.AddTorque(7f, ForceMode2D.Impulse);
            Destroy(gameObject, 2f);
        }
    }


    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        target = FindObjectOfType<TargetController>().transform;
    }
}
