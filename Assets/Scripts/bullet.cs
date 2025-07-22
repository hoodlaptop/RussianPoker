using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Damage")]
    public float pistolDamage_Head = 2f;
    public float pistolDamage_Body = 1f;
    public float shotgunDamage_Head = 0.6f;
    public float shotgunDamage_Body = 0.3f;
    public float SMGDamage_Head = 2f;
    public float SMGDamage_Body = 1f;

    [Header("Bullet Settings")]
    public string identity = "default";
    public int gunNum = 1;
    public Vector3 startPosition;
    public bool active = false;

    private void Update()
    {
        // active 상태일때만
        if (active)
        {
            if (Vector3.Distance(startPosition, transform.position) > 50f)
            {
                Debug.Log("삭제");
                active = false;
                gameObject.SetActive(false);
            }
        }
    }

    public float HeadDamage()
    {
        switch(gunNum)
        {
            case 1:
                return pistolDamage_Head;
            case 2:
                return shotgunDamage_Head;
            case 3:
                return SMGDamage_Head;
            default:
                return 0f;
        }
    }

    public float BodyDamage()
    {
        switch (gunNum)
        {
            case 1:
                return pistolDamage_Body;
            case 2:
                return shotgunDamage_Body;
            case 3:
                return SMGDamage_Body;
            default:
                return 0f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //같은 총알과 부딫힌게 아니라면 삭제
        if (!collision.gameObject.CompareTag("bullet"))
        {
            gameObject.SetActive(false);
        }
    }

}
