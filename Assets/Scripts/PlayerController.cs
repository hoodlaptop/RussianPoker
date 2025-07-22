using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpHeight = 1.5f;

    [Header("Air Movement")]
    public float airControlMultiplier = 0.3f; // 공중 제어력 비율

    [Header("Jump Settings")]
    public bool doubleJumpEnabled = false;
    private int jumpCounter = 0;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    private GameObject playerHpBar;

    private int HPbarSize = 500;
    private Vector3 playerRespawn;

    private Camera playerCamera;
    private Rigidbody playerRigidbody;
    private float xRotation = 0f;
    private bool isGrounded;
    public float mouseX;
    public float mouseY;
    private UnityEngine.UI.Image redPanel;
    private GunManager gunManager;

    private float fadeDuration = 0.1f;

    void Start()
    {
        // 리지드바디 컴포넌트 가져오기
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true; // 물리적 회전 방지

        // 자식 오브젝트에서 카메라 가져오기
        playerCamera = GetComponentInChildren<Camera>();

        // 마우스 커서 잠금
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        // Hp바 찾기
        playerHpBar = GameObject.Find("HP bar").gameObject;

        //피격 색 Panel 찾기
        GameObject panel = GameObject.Find("RedPanel").gameObject;
        redPanel = panel.GetComponent<UnityEngine.UI.Image>();
        Debug.Log(redPanel.name);

        // GunManager 찾기
        GameObject manager = GameObject.Find("GunManager");
        gunManager = manager.GetComponent<GunManager>();

        //리스폰 위치
        playerRespawn = transform.position;
    }

    void Update()
    {
        // 땅에 닿아 있는지 레이캐스트로 확인
        isGrounded = Physics.Raycast(transform.position + Vector3.down * 1.0f, Vector3.down, 0.6f);

        HandleMouseLook();
        HandleMovement();
        HandleJump();
        HandleShoot();
    }

    void HandleMouseLook()
    {
        // 마우스 입력 받기
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 좌우 회전(Y축) - 플레이어 몸체 회전
        transform.Rotate(Vector3.up * mouseX);

        // 상하 회전(X축) - 카메라만 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle); // 시야각 제한
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        // 입력 받기
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");

        // 플레이어 기준 이동 방향 계산
        Vector3 moveInput = transform.right * hAxis + transform.forward * vAxis;
        moveInput = moveInput.normalized; // 대각선 이동 속도 정규화

        if (isGrounded)
        {
            // 땅에서는 즉시 반응
            Vector3 targetVelocity = moveInput * walkSpeed;
            playerRigidbody.velocity = new Vector3(targetVelocity.x, playerRigidbody.velocity.y, targetVelocity.z);

            // 입력이 없으면 마찰력으로 빠르게 감속
            if (moveInput.magnitude == 0)
            {
                Vector3 friction = new Vector3(playerRigidbody.velocity.x * -8f, 0, playerRigidbody.velocity.z * -8f);
                playerRigidbody.AddForce(friction, ForceMode.Force);
            }
        }
        else
        {
            // 공중에서는 제한된 제어
            AirMovement(moveInput);
        }
    }

    void AirMovement(Vector3 moveInput)
    {
        // 현재 수평 속도 가져오기
        Vector3 currentHorizontal = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);

        // 공중 제어력만큼 힘 계산
        Vector3 airForce = moveInput * walkSpeed * airControlMultiplier;

        // 현재 속도와 입력 방향의 내적으로 가속/감속 결정
        float currentSpeedInInputDirection = Vector3.Dot(currentHorizontal.normalized, moveInput);

        // 최대 속도보다 느릴 때만 힘 추가
        if (currentSpeedInInputDirection < walkSpeed)
        {
            playerRigidbody.AddForce(airForce, ForceMode.Force);
        }

        // 최대 속도 제한
        Vector3 newHorizontal = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
        if (newHorizontal.magnitude > walkSpeed)
        {
            newHorizontal = newHorizontal.normalized * walkSpeed;
            playerRigidbody.velocity = new Vector3(newHorizontal.x, playerRigidbody.velocity.y, newHorizontal.z);
        }
    }

    void HandleJump()
    {
        // 땅에 착지하면 점프 카운터 리셋
        if (isGrounded && playerRigidbody.velocity.y <= -0.1f)
        {
            jumpCounter = 0;
        }

        // 스페이스바 입력 시 점프
        if (Input.GetButtonDown("Jump"))
        {
            // 첫 번째 점프 (항상 가능)
            if (jumpCounter == 0)
            {
                // Y축 속도 초기화 후 점프력 적용
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                playerRigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                jumpCounter++;
            }
            // 더블 점프 (아이템 활성화 시에만)
            else if (jumpCounter == 1 && doubleJumpEnabled)
            {
                // Y축 속도 초기화 후 점프력 적용
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
                playerRigidbody.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
                jumpCounter++;
            }
        }
    }

    void HandleShoot()
    {
        //마우스 왼쪽 버튼이 눌렸을때
        if (Input.GetMouseButtonDown(0))
        {

            gunManager.Shot(playerCamera.transform.position, playerCamera.transform.forward.normalized);
            
        }
        //마우스를 계속 누르고있는데 선택된 총이 SMG라면 Shot 호출
        if (Input.GetMouseButton(0) && gunManager.gunNum == 3)
        {

            gunManager.Shot(playerCamera.transform.position, playerCamera.transform.forward.normalized);

        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //충돌 물체의 태그가 bullet일 경우
        if (collision.gameObject.CompareTag("bullet"))
        {
            RectTransform rt = playerHpBar.GetComponent<RectTransform>();
            Vector2 size = rt.sizeDelta;

            //피격 효과
            StartCoroutine(FadeInOut());

            //체력의 30% 깎음
            size.x = size.x - (HPbarSize * 0.3f);

            //0 이하면 사망, 초기화 후 리스폰
            if (size.x < 0)
            {
                size.x = HPbarSize;
                transform.position = playerRespawn;
            }
            rt.sizeDelta = size;
            Debug.Log("맞음");
        }
    }


    IEnumerator FadeInOut()
    {
        // 페이드 인 (투명 → 불투명)
        yield return StartCoroutine(Fade(0f, 0.34f));
        // 잠시 대기
        yield return new WaitForSeconds(0.1f);    
        // 페이드 아웃 (불투명 → 투명)
        yield return StartCoroutine(Fade(0.34f, 0f));
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        Color color = redPanel.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            redPanel.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 최종 알파값 설정
        redPanel.color = new Color(color.r, color.g, color.b, endAlpha);
    }

}


