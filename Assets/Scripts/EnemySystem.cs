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
    public float rotationSpeed = 5f; //ȸ�����ǵ�

    [Header("Air Movement")]
    public float airControlMultiplier = 0.3f; // ���� ����� ����

    [Header("Enemy Settings")]
    public string identity = "enemy";
    public float enemyHP = 10f;
    public Vector3 respawnSpot = new Vector3(10, 1, 0);
    public float detectionDistance = 20f; //���̰��� �Ÿ�

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

                        // bullet ���� ����
                        bullet.GetComponent<Bullet>().identity = identity;
                        bullet.GetComponent<Bullet>().gunNum = 1; // pistol

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
        //�浹 ��ü�� �±װ� Ground�� ���
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        //�浹 ��ü�� �±װ� bullet�� ���
        if (collision.gameObject.CompareTag("bullet"))
        {
            //identity�� �ٸ��ٸ�
            if (collision.gameObject.GetComponent<Bullet>().identity != identity)
            {
                //������
                //�浹�� object�� layer ã�� (Head or Body)
                GameObject hitObject = null;
                foreach (ContactPoint contact in collision.contacts)
                {
                    hitObject = contact.thisCollider.gameObject;
                }
                // Head / Body ���ο� ���� ������ �����ͼ� ����
                switch (LayerMask.LayerToName(hitObject.layer))
                {
                    case "Head":
                        currentEnemyHP -= collision.gameObject.GetComponent<Bullet>().HeadDamage();
                        Debug.Log("�Ӹ�" + collision.gameObject.GetComponent<Bullet>().HeadDamage().ToString());
                        break;

                    case "Body":
                        currentEnemyHP -= collision.gameObject.GetComponent<Bullet>().BodyDamage();
                        Debug.Log("��" + collision.gameObject.GetComponent<Bullet>().BodyDamage().ToString());
                        break;

                    default:

                        break;
                }


                //0 ���ϸ� ���ó��, ������
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
        //�������� ��ü�� �±װ� Ground�� ���
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    private void Death()
    {
        //ĳ���� ������ �ʰ��ϱ�
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        head.GetComponent<MeshRenderer>().enabled = false;
        head.GetComponent<SphereCollider>().enabled = false;
    }


    IEnumerator Respawn()
    {
        //3�� ��
        yield return new WaitForSeconds(3f);

        //HP �ʱ�ȭ
        currentEnemyHP = enemyHP;

        //������ ��ҷ� �̵�
        transform.position = respawnSpot;

        //���̰� �ϱ�
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
