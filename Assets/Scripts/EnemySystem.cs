using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemySystem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 0.0005f;
    public float rotationSpeed = 5f; //ȸ�����ǵ�

    [Header("Air Movement")]
    public float airControlMultiplier = 0.3f; // ���� ����� ����

    [Header("Enemy Settings")]
    public GameObject HPBar;
    public RectTransform HPBarGauge;
    public float enemyHP = 10f;
    public Vector3 respawnSpot = new Vector3(10, 1, 0);
    public float detectionDistance = 20f; //���̰��� �Ÿ�

    private Vector2 HPBarSize;
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
        HPBarSize = HPBarGauge.sizeDelta;
    }


    void Update()
    {

        // ���� ��� �ִ��� ����ĳ��Ʈ�� Ȯ��
        isGrounded = Physics.Raycast(transform.position + Vector3.down * 1.0f, Vector3.down, 0.6f);

        HandleMovement();
        TargetPlayer();

    }



    void HandleMovement()
    {
        //�÷��̾� �������� ȸ��
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
        
        //�÷��̾� �������� �̵�
        if (isGrounded)
        {
            // �������� ��� ����
            Vector3 targetVelocity = direction * walkSpeed;
            rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

            // �Է��� ������ ���������� ������ ����
            if (direction.magnitude == 0)
            {
                Vector3 friction = new Vector3(rb.velocity.x * -8f, 0, rb.velocity.z * -8f);
                rb.AddForce(friction, ForceMode.Force);
            }
        }
        else
        {
            // ���߿����� ���ѵ� ����
            AirMovement(direction);
        }

    }

    void AirMovement(Vector3 moveInput)
    {
        // ���� ���� �ӵ� ��������
        Vector3 currentHorizontal = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // ���� ����¸�ŭ �� ���
        Vector3 airForce = moveInput * walkSpeed * airControlMultiplier;

        // ���� �ӵ��� �Է� ������ �������� ����/���� ����
        float currentSpeedInInputDirection = Vector3.Dot(currentHorizontal.normalized, moveInput);

        // �ִ� �ӵ����� ���� ���� �� �߰�
        if (currentSpeedInInputDirection < walkSpeed)
        {
            rb.AddForce(airForce, ForceMode.Force);
        }

        // �ִ� �ӵ� ����
        Vector3 newHorizontal = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (newHorizontal.magnitude > walkSpeed)
        {
            newHorizontal = newHorizontal.normalized * walkSpeed;
            rb.velocity = new Vector3(newHorizontal.x, rb.velocity.y, newHorizontal.z);
        }
    }

    void TargetPlayer()
    {
        //�÷��̾� ����
        //���� ���� ��ġ
        Vector3 rayPosition = transform.position;
        rayPosition.y = rayPosition.y + 0.648f;
        //���� ����
        Vector3 rayDirection = transform.forward.normalized;



        Ray ray = new Ray(rayPosition, rayDirection); // �������� ���� ��
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {

                // �߻� �� ���ƿ���
                if (shootTerm)
                {
                    Debug.Log("�߻�");
                    //�Ѿ� ������ƮǮ��
                    GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
                    if (bullet != null)
                    {
                        bullet.GetComponent<Bullet>().active = true;
                        bullet.GetComponent<Bullet>().startPosition = rayPosition + rayDirection;
                        bullet.SetActive(true); // activate it
                        //�Ѿ� ���� ��ġ
                        bullet.transform.position = rayPosition + rayDirection;
                        // �Ѿ� ���� �ʱ�ȭ
                        bullet.transform.rotation = Quaternion.LookRotation(rayDirection);
                        Debug.Log("�� �Ѿ� �߻�");
                    }

                    // Rigidbody�� �ӵ� �ֱ�
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

        // ������ ���� ���̰� �ϱ�
        Debug.DrawRay(rayPosition, rayDirection * detectionDistance, Color.red);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //�浹 ��ü�� �±װ� bullet�� ���
        if (collision.gameObject.CompareTag("bullet"))
        {
            //������
            currentEnemyHP -= 2f;

            //HP�� ũ�� ����
            Vector2 size = HPBarGauge.sizeDelta;
            Debug.Log((currentEnemyHP / enemyHP).ToString());
            size.x = HPBarSize.x * (currentEnemyHP / enemyHP); // HP ������ ������ �̹��� ũ�� ����
            HPBarGauge.sizeDelta = size; //����

            //0 ���ϸ� ���ó��, ������
            if (currentEnemyHP <= 0)
            {
                Death();
                StartCoroutine(Respawn());
            }
        }
    }

    private void Death()
    {
        //ĳ���� ������ �ʰ��ϱ�
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        HPBar.SetActive(false);
    }

    IEnumerator Respawn()
    {
        //3�� ��
        yield return new WaitForSeconds(3f);

        //HP �ʱ�ȭ
        currentEnemyHP = enemyHP;
        HPBarGauge.sizeDelta = HPBarSize;

        //������ ��ҷ� �̵�
        transform.position = respawnSpot;

        //���̰� �ϱ�
        HPBar.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
    }

    IEnumerator Timer(float duration)
    {
        yield return new WaitForSeconds(duration);
        shootTerm = true;
    }

}
