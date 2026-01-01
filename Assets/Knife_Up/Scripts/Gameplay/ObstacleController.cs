using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour {

    private Transform target;
    private EdgeCollider2D edgeCollider = null;
    private Rigidbody2D rigid = null;
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
            edgeCollider.enabled = false;
            Vector2 forceDir = (transform.position - target.position).normalized * 6f;
            rigid.bodyType = RigidbodyType2D.Dynamic;
            rigid.AddForceAtPosition(forceDir, target.position, ForceMode2D.Impulse);
            rigid.AddTorque(10f, ForceMode2D.Impulse);
            Destroy(gameObject, 2f);
        }
    }

    void Start () {
        edgeCollider = GetComponent<EdgeCollider2D>();
        rigid = GetComponent<Rigidbody2D>();
        target = FindObjectOfType<TargetController>().transform;
	}

}
