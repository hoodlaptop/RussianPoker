using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> bulletImages = new List<GameObject>();
    [SerializeField]
    private GameObject gunNameObject;
    [SerializeField]
    private GameObject reloadingObject;

    private TextMeshProUGUI gunName;
    private TextMeshProUGUI reloadingText;
    //총알 속도
    private float pistolBulletSpeed = 70;
    private float shotgunBulletSpeed = 60;
    private float SMGBulletSpeed = 80;

    //선택된 총 숫자
    public int gunNum = 1;

    //남은 탄수
    public int pistolBullet = 6;
    public int shotgunBullet = 6;
    public int SMGBullet = 30;

    //산탄총 퍼짐 정도
    public float spreadAngle = 5f;

    //쿨타임
    private bool pistolTerm = true;
    private bool shotgunTerm = true;
    private bool SMGTerm = true;

    //재장전 중
    private bool reloading = false;

    private void Start()
    {
        gunName = gunNameObject.GetComponent<TextMeshProUGUI>();
        for (int i = 6; i < bulletImages.Count; i++)
        {
            bulletImages[i].SetActive(false);
        }
        reloadingText = reloadingObject.GetComponent<TextMeshProUGUI>();
        reloadingText.enabled = false;
    }
    private void Update()
    {
        // 재장전 중이 아닐 때
        if (!reloading)
        {
            // 1 2 3 키 입력 감지해서 총 바꿈
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gunNum = 1;
                gunName.text = "Pistol";
                //남아있는 bullet 수만큼 표시
                for (int i = 0; i < pistolBullet; i++)
                {
                    bulletImages[i].SetActive(true);
                }
                for (int i = pistolBullet; i < bulletImages.Count; i++)
                {
                    bulletImages[i].SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                gunNum = 2;
                gunName.text = "Shotgun";
                //남아있는 bullet 수만큼 표시
                for (int i = 0; i < shotgunBullet; i++)
                {
                    bulletImages[i].SetActive(true);
                }
                for (int i = shotgunBullet; i < bulletImages.Count; i++)
                {
                    bulletImages[i].SetActive(false);
                }
            }
            if (Input.GetKey(KeyCode.Alpha3))
            {
                gunNum = 3;
                gunName.text = "SMG";
                //남아있는 bullet 수만큼 표시
                for (int i = 0; i < SMGBullet; i++)
                {
                    bulletImages[i].SetActive(true);
                }
                for (int i = SMGBullet; i < bulletImages.Count; i++)
                {
                    bulletImages[i].SetActive(false);
                }
            }

            //R이 눌렸을때 선택된 총 재장전
            if (Input.GetKeyDown(KeyCode.R))
            {
                HandleReload();
            }
        }
    }

    public void Shot(Vector3 startPosition, Vector3 rotate, string identity)
    {
        // 재장전 중이 아닐 때
        if (!reloading)
        {
            //쿨타임 돌아왔을때만 호출
            switch (gunNum)
            {
                case 1:
                    if (pistolTerm)
                    {
                        PistolShot(startPosition, rotate, identity);
                    }
                    break;
                case 2:
                    if (shotgunTerm)
                    {
                        ShotgunShot(startPosition, rotate, identity);
                    }
                    break;
                case 3:
                    if (SMGTerm)
                    {
                        SMGShot(startPosition, rotate, identity);
                    }
                    break;
            }
        }
    }

    private void PistolShot(Vector3 startPosition, Vector3 rotate, string identity)
    {
        //총알이 남아있다면
        if (pistolBullet > 0)
        {
            //총알 오브젝트풀링
            GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
            if (bullet != null)
            {
                Bullet B = bullet.GetComponent<Bullet>();
                B.active = true;
                B.identity = identity;
                B.gunNum = 1;
                bullet.GetComponent<Bullet>().startPosition = startPosition + rotate;
                bullet.SetActive(true); // activate it
                //총알 생성 위치
                bullet.transform.position = startPosition + rotate;
                bullet.transform.rotation = Quaternion.LookRotation(rotate);

                // Rigidbody에 속도 주기
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = rotate * pistolBulletSpeed;
                }
            }
            //총알 개수 -1
            pistolBullet--;

            pistolTerm = false;
            //일정 시간 후 pistolterm을 true로 바꿔주는 코루틴 시작
            StartCoroutine(pistoltime());

            //남아있는 bullet 수만큼 표시
            for (int i = 0; i < pistolBullet; i++)
            {
                bulletImages[i].SetActive(true);
            }
            for (int i = pistolBullet; i < bulletImages.Count; i++)
            {
                bulletImages[i].SetActive(false);
            }
        }
    }

    private void ShotgunShot(Vector3 startPosition, Vector3 rotate, string identity)
    {
        //총알이 남아있다면
        if (shotgunBullet > 0)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
                if (bullet != null)
                {
                    // 퍼짐 방향 만들기
                    Vector3 spread = Quaternion.Euler(
                        Random.Range(-spreadAngle, spreadAngle),
                        Random.Range(-spreadAngle, spreadAngle),
                        0f) * rotate;

                    Bullet B = bullet.GetComponent<Bullet>();
                    B.active = true;
                    B.identity = identity;
                    B.gunNum = 2;
                    bullet.GetComponent<Bullet>().startPosition = startPosition + spread;
                    bullet.SetActive(true);
                    bullet.transform.position = startPosition + spread;
                    bullet.transform.rotation = Quaternion.LookRotation(spread);

                    Rigidbody rb = bullet.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = spread.normalized * shotgunBulletSpeed;
                    }
                }
            }

            //총알 개수 -1
            shotgunBullet--;

            shotgunTerm = false;
            //일정 시간 후 shotgunterm을 true로 바꿔주는 코루틴 시작
            StartCoroutine(shotguntime());

            //남아있는 bullet 수만큼 표시
            for (int i = 0; i < shotgunBullet; i++)
            {
                bulletImages[i].SetActive(true);
            }
            for (int i = shotgunBullet; i < bulletImages.Count; i++)
            {
                bulletImages[i].SetActive(false);
            }
        }
    }

    private void SMGShot(Vector3 startPosition, Vector3 rotate, string identity)
    {
        //총알이 남아있다면
        if (SMGBullet > 0)
        {
            //총알 오브젝트풀링
            GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
            if (bullet != null)
            {
                Bullet B = bullet.GetComponent<Bullet>();
                B.active = true;
                B.identity = identity;
                B.gunNum = 3;
                bullet.GetComponent<Bullet>().startPosition = startPosition + rotate;
                bullet.SetActive(true); // activate it
                //총알 생성 위치
                bullet.transform.position = startPosition + rotate;
                bullet.transform.rotation = Quaternion.LookRotation(rotate);

                // Rigidbody에 속도 주기
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = rotate * SMGBulletSpeed;
                }
            }
            //총알 개수 -1
            SMGBullet--;

            SMGTerm = false;
            //일정 시간 후 SNGterm을 true로 바꿔주는 코루틴 시작
            StartCoroutine(SMGtime());

            //남아있는 bullet 수만큼 표시
            for (int i = 0; i < SMGBullet; i++)
            {
                bulletImages[i].SetActive(true);
            }
            for (int i = SMGBullet; i < bulletImages.Count; i++)
            {
                bulletImages[i].SetActive(false);
            }
        }
    }

    void HandleReload()
    {
        //선택된 총에 따라 초기화하고 이미지도 다시 설정
        switch (gunNum)
        {
            case 1:
                StartCoroutine(PistolReload());
                break;
            case 2:
                StartCoroutine(ShotgunReload());
                break;
            case 3:
                StartCoroutine(SMGReload());
                break;
        }
    }

    IEnumerator pistoltime()
    {
        // 0.5초 후 재발사 가능
        yield return new WaitForSeconds(0.5f);
        pistolTerm = true;
    }
    IEnumerator shotguntime()
    {
        // 1초 후 재발사 가능
        yield return new WaitForSeconds(1f);
        shotgunTerm = true;
    }
    IEnumerator SMGtime()
    {
        // 0.1초 후 재발사 가능
        yield return new WaitForSeconds(0.1f);
        SMGTerm = true;
    }

    IEnumerator PistolReload()
    {
        reloading = true;
        reloadingText.enabled = true;
        //재장전 시간
        yield return new WaitForSeconds(1f);

        pistolBullet = 6;

        //남아있는 bullet 수만큼 표시
        for (int i = 0; i < pistolBullet; i++)
        {
            bulletImages[i].SetActive(true);
        }
        for (int i = pistolBullet; i < bulletImages.Count; i++)
        {
            bulletImages[i].SetActive(false);
        }

        reloadingText.enabled = false;
        reloading = false;
    }

    IEnumerator ShotgunReload()
    {
        reloading = true;
        reloadingText.enabled = true;
        //재장전 시간
        yield return new WaitForSeconds(1f);

        shotgunBullet = 6;

        //남아있는 bullet 수만큼 표시
        for (int i = 0; i < shotgunBullet; i++)
        {
            bulletImages[i].SetActive(true);
        }
        for (int i = shotgunBullet; i < bulletImages.Count; i++)
        {
            bulletImages[i].SetActive(false);
        }

        reloadingText.enabled = false;
        reloading = false;
    }

    IEnumerator SMGReload()
    {
        reloading = true;
        reloadingText.enabled = true;
        //재장전 시간
        yield return new WaitForSeconds(1f);

        SMGBullet = 30;

        //남아있는 bullet 수만큼 표시
        for (int i = 0; i < SMGBullet; i++)
        {
            bulletImages[i].SetActive(true);
        }
        for (int i = SMGBullet; i < bulletImages.Count; i++)
        {
            bulletImages[i].SetActive(false);
        }

        reloadingText.enabled = false;
        reloading = false;
    }
}
