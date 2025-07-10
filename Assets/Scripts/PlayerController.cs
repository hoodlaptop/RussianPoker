using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    
    private float xRotation = 0f;
    
    private Camera playerCamera;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        
        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // 좌우 회전(Y축)
        transform.Rotate(Vector3.up * mouseX);
        
        // 상하 회전(X축)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        
        Vector3 moveVec = transform.right * hAxis + transform.forward * vAxis;
        
        transform.position += moveVec * (walkSpeed * Time.deltaTime);
    }
}
