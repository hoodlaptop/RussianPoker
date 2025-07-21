using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem : MonoBehaviour
{

    private GameObject rayOrigin;
    public float detectionDistance = 20f; //레이감지 거리

    private void Start()
    {
        rayOrigin = new GameObject();
    }

    // Update is called once per frame
    void Update()
    {
        //레이 시작 위치
        Vector3 rayPosition = new Vector3(transform.position.x, 0.648f, transform.position.z);
        rayOrigin.transform.position = rayPosition;

        Ray ray = new Ray(rayOrigin.transform.position, rayOrigin.transform.forward); // 정면으로 레이 쏨
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("플레이어 감지됨!");
                // 여기에 추격, 공격, 시선 회전 등 로직 추가
            }
        }

        // 씬에서 레이 보이게 하기
        Debug.DrawRay(rayOrigin.transform.position, rayOrigin.transform.forward * detectionDistance, Color.red);
    }


}
