using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private PlayerController playerController;
    private Camera camera;


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
        // active �����϶���
        if (active)
        {
            if (Vector3.Distance(startPosition, transform.position) > 50f)
            {
                Debug.Log("����");
                active = false;
                gameObject.SetActive(false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //���� �Ѿ˰� �΋H���� �ƴ϶�� ����
        if (!collision.gameObject.CompareTag("bullet"))
        {
            gameObject.SetActive(false);
        }
    }

}
