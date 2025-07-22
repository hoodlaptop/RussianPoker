using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemySystem : MonoBehaviour
{
    [SerializeField]
    private GameObject head;

    [Header("Movement Settings")]
    public float walkSpeed = 0.0005f;
    public float rotationSpeed = 5f; //회전스피드

    [Header("Air Movement")]
    public float airControlMultiplier = 0.3f; // 공중 제어력 비율

    [Header("Enemy Settings")]
    public string identity = "enemy";
    public float enemyHP = 10f;
    public Vector3 respawnSpot = new Vector3(10, 1, 0);
    public float detectionDistance = 20f; //레이감지 거리

    private float currentEnemyHP;

    private PlayerController player;
    private Rigidbody rb;

    private bool isGrounded;
    private bool shootTerm = true;
    private float bulletSpeed = 100f;
    private void Start()
    {
        player = FindObjectOfType<PlayerController>();
        rb = GetComponent<Rigidbody>();
        currentEnemyHP = enemyHP;
    }


    void Update()
    {

        HandleMovement();
        TargetPlayer();

    }



    void HandleMovement()
    {
        //플레이어 방향으로 회전
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
        
        //플레이어 방향으로 이동
        if (isGrounded)
        {
            // 땅에서는 즉시 반응
            Vector3 targetVelocity = direction * walkSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            // 입력이 없으면 마찰력으로 빠르게 감속
            if (direction.magnitude == 0)
            {
                Vector3 friction = new Vector3(rb.velocity.x * -8f, 0, rb.velocity.z * -8f);
                rb.AddForce(friction, ForceMode.Force);
            }
        }
        else
        {
            // 공중에서는 제한된 제어
            AirMovement(direction);
        }

    }

    void AirMovement(Vector3 moveInput)
    {
        // 현재 수평 속도 가져오기
        Vector3 currentHorizontal = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // 공중 제어력만큼 힘 계산
        Vector3 airForce = moveInput * walkSpeed * airControlMultiplier;

        // 현재 속도와 입력 방향의 내적으로 가속/감속 결정
        float currentSpeedInInputDirection = Vector3.Dot(currentHorizontal.normalized, moveInput);

        // 최대 속도보다 느릴 때만 힘 추가
        if (currentSpeedInInputDirection < walkSpeed)
        {
            rb.AddForce(airForce, ForceMode.Force);
        }

        // 최대 속도 제한
        Vector3 newHorizontal = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (newHorizontal.magnitude > walkSpeed)
        {
            newHorizontal = newHorizontal.normalized * walkSpeed;
            rb.velocity = new Vector3(newHorizontal.x, rb.velocity.y, newHorizontal.z);
        }
    }

    void TargetPlayer()
    {
        //플레이어 감지
        //레이 시작 위치
        Vector3 rayPosition = transform.position;
        rayPosition.y = rayPosition.y + 0.648f;
        //레이 방향
        Vector3 rayDirection = transform.forward.normalized;



        Ray ray = new Ray(rayPosition, rayDirection); // 정면으로 레이 쏨
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {

                // 발사 텀 돌아오면
                if (shootTerm)
                {
                    Debug.Log("발사");
                    //총알 오브젝트풀링
                    GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
                    if (bullet != null)
                    {
                        bullet.GetComponent<Bullet>().active = true;

                        // bullet 정보 설정
                        bullet.GetComponent<Bullet>().identity = identity;
                        bullet.GetComponent<Bullet>().gunNum = 1; // pistol

                        bullet.GetComponent<Bullet>().startPosition = rayPosition + rayDirection;
                        bullet.SetActive(true); // activate it
                        //총알 생성 위치
                        bullet.transform.position = rayPosition + rayDirection;
                        // 총알 방향 초기화
                        bullet.transform.rotation = Quaternion.LookRotation(rayDirection);
                        Debug.Log("적 총알 발사");
                    }

                    // Rigidbody에 속도 주기
                    Rigidbody rb = bullet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = rayDirection * bulletSpeed;
                    }

                    shootTerm = false;
                    StartCoroutine(Timer(5f));
                }

            }
        }

        // 씬에서 레이 보이게 하기
        Debug.DrawRay(rayPosition, rayDirection * detectionDistance, Color.red);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //충돌 물체의 태그가 Ground일 경우
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        //충돌 물체의 태그가 bullet일 경우
        if (collision.gameObject.CompareTag("bullet"))
        {
            //identity가 다르다면
            if (collision.gameObject.GetComponent<Bullet>().identity != identity)
            {
                //데미지
                //충돌한 object의 layer 찾기 (Head or Body)
                GameObject hitObject = null;
                foreach (ContactPoint contact in collision.contacts)
                {
                    hitObject = contact.thisCollider.gameObject;
                }
                // Head / Body 여부에 따라 데미지 가져와서 감소
                switch (LayerMask.LayerToName(hitObject.layer))
                {
                    case "Head":
                        currentEnemyHP -= collision.gameObject.GetComponent<Bullet>().HeadDamage();
                        Debug.Log("머리" + collision.gameObject.GetComponent<Bullet>().HeadDamage().ToString());
                        break;

                    case "Body":
                        currentEnemyHP -= collision.gameObject.GetComponent<Bullet>().BodyDamage();
                        Debug.Log("몸" + collision.gameObject.GetComponent<Bullet>().BodyDamage().ToString());
                        break;

                    default:

                        break;
                }


                //0 이하면 사망처리, 리스폰
                if (currentEnemyHP <= 0)
                {
                    Death();
                    StartCoroutine(Respawn());
                }
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        //떨어지는 물체의 태그가 Ground일 경우
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Death()
    {
        //캐릭터 보이지 않게하기
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        head.GetComponent<MeshRenderer>().enabled = false;
        head.GetComponent<SphereCollider>().enabled = false;
    }


    IEnumerator Respawn()
    {
        //3초 후
        yield return new WaitForSeconds(3f);

        //HP 초기화
        currentEnemyHP = enemyHP;

        //리스폰 장소로 이동
        transform.position = respawnSpot;

        //보이게 하기
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
        head.GetComponent<MeshRenderer>().enabled = true;
        head.GetComponent<SphereCollider>().enabled = true;
    }

    IEnumerator Timer(float duration)
    {
        yield return new WaitForSeconds(duration);
        shootTerm = true;
    }

}
