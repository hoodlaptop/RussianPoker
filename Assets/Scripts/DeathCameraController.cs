using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    void Update()
    {
        transform.position = player.transform.position + new Vector3(10, 5, 0);
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }
}
