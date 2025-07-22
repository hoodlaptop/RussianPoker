using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerController playerController;
    private Camera camera;

    public string name = "";
    public int gunNum = 1;
    public Vector3 startPosition;
    public bool active = false;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        playerController = FindObjectOfType<PlayerController>();
        camera = FindObjectOfType<Camera>();
    }

    private void Update()
    {
        // active 상태일때만
        if (active)
        {
            if (Vector3.Distance(startPosition, transform.position) > 50f)
            {
                Debug.Log("삭제");
                active = false;
                gameObject.SetActive(false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //같은 총알과 부딫힌게 아니라면 삭제
        if (!collision.gameObject.CompareTag("bullet"))
        {
            gameObject.SetActive(false);
        }
    }

}
