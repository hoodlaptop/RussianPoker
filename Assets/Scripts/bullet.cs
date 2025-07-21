using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField]
    private float bulletSpeed = 10.0f;
    private Rigidbody rb;
    private PlayerController playerController;
    private Camera camera;
    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        playerController = FindObjectOfType<PlayerController>();
        camera = FindObjectOfType<Camera>();
    }

    private void Update()
    {
        //�Ѿ� ī�޶� �������� ����
        rb.velocity = camera.transform.forward.normalized * bulletSpeed;

    }

    void OnCollisionEnter(Collision collision)
    {
        gameObject.SetActive(false);
    }

}
