using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;

    [Header("Jump Settings")]
    public bool doubleJumpEnabled = false;
    private int jumpCounter = 0;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    
    private Camera playerCamera;
    private Rigidbody playerRigidbody;
    private float xRotation = 0f;
    private bool isGrounded;
    
    void Start()
    {
        // Rigidbody 가져오기
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true; // 물리적 회전 방지
        
        // 카메라 가져오기
        playerCamera = GetComponentInChildren<Camera>();
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 땅에 닿아 있는지 확인
        isGrounded = Physics.Raycast(transform.position + Vector3.down * 1.0f, Vector3.down, 0.6f);
        
        HandleMouseLook();
        HandleMovement();
        HandleJump();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // 좌우 회전(Y축)
        transform.Rotate(Vector3.up * mouseX);
        
        // 상하 회전(X축) -> 카메라만 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        // 입력 받기
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        
        // 이동 방향 계산
        Vector3 moveVec = transform.right * hAxis + transform.forward * vAxis;
        
        // 이동 적용
        transform.position += moveVec * (walkSpeed * Time.deltaTime);
    }

    void HandleJump()
    {
        // 땅에 닿으면 점프 카운트 리셋
        if (isGrounded && playerRigidbody.velocity.y <= -0.1f)
        {
            jumpCounter = 0;
        }
        
        if (Input.GetButtonDown("Jump"))
        {
            if (jumpCounter == 0)
            {
                playerRigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                jumpCounter++;
            }
            // 더블 점프가 true일때만 가능(아이템)
            else if (jumpCounter == 1 && doubleJumpEnabled)
            {
                playerRigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                jumpCounter++;
            }
        }
    }
}
