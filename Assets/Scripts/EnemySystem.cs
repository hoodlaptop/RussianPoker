using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : MonoBehaviour
{

    private GameObject rayOrigin;
    public float detectionDistance = 20f; //���̰��� �Ÿ�

    private void Start()
    {
        rayOrigin = new GameObject();
    }

    // Update is called once per frame
    void Update()
    {
        //���� ���� ��ġ
        Vector3 rayPosition = new Vector3(transform.position.x, 0.648f, transform.position.z);
        rayOrigin.transform.position = rayPosition;

        Ray ray = new Ray(rayOrigin.transform.position, rayOrigin.transform.forward); // �������� ���� ��
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("�÷��̾� ������!");
                // ���⿡ �߰�, ����, �ü� ȸ�� �� ���� �߰�
            }
        }

        // ������ ���� ���̰� �ϱ�
        Debug.DrawRay(rayOrigin.transform.position, rayOrigin.transform.forward * detectionDistance, Color.red);
    }


}
