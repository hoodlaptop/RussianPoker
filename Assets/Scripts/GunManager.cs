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
    //�Ѿ� �ӵ�
    private float pistolBulletSpeed = 70;
    private float shotgunBulletSpeed = 60;
    private float SMGBulletSpeed = 80;

    //���õ� �� ����
    public int gunNum = 1;

    //���� ź��
    public int pistolBullet = 6;
    public int shotgunBullet = 6;
    public int SMGBullet = 30;

    //��ź�� ���� ����
    public float spreadAngle = 5f;

    //��Ÿ��
    private bool pistolTerm = true;
    private bool shotgunTerm = true;
    private bool SMGTerm = true;

    //������ ��
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
        // ������ ���� �ƴ� ��
        if (!reloading)
        {
            // 1 2 3 Ű �Է� �����ؼ� �� �ٲ�
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gunNum = 1;
                gunName.text = "Pistol";
                //�����ִ� bullet ����ŭ ǥ��
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
                //�����ִ� bullet ����ŭ ǥ��
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
                //�����ִ� bullet ����ŭ ǥ��
                for (int i = 0; i < SMGBullet; i++)
                {
                    bulletImages[i].SetActive(true);
                }
                for (int i = SMGBullet; i < bulletImages.Count; i++)
                {
                    bulletImages[i].SetActive(false);
                }
            }

            //R�� �������� ���õ� �� ������
            if (Input.GetKeyDown(KeyCode.R))
            {
                HandleReload();
            }
        }
    }

    public void Shot(Vector3 startPosition, Vector3 rotate, string identity)
    {
        // ������ ���� �ƴ� ��
        if (!reloading)
        {
            //��Ÿ�� ���ƿ������� ȣ��
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
        //�Ѿ��� �����ִٸ�
        if (pistolBullet > 0)
        {
            //�Ѿ� ������ƮǮ��
            GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
            if (bullet != null)
            {
                Bullet B = bullet.GetComponent<Bullet>();
                B.active = true;
                B.identity = identity;
                B.gunNum = 1;
                bullet.GetComponent<Bullet>().startPosition = startPosition + rotate;
                bullet.SetActive(true); // activate it
                //�Ѿ� ���� ��ġ
                bullet.transform.position = startPosition + rotate;
                bullet.transform.rotation = Quaternion.LookRotation(rotate);

                // Rigidbody�� �ӵ� �ֱ�
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = rotate * pistolBulletSpeed;
                }
            }
            //�Ѿ� ���� -1
            pistolBullet--;

            pistolTerm = false;
            //���� �ð� �� pistolterm�� true�� �ٲ��ִ� �ڷ�ƾ ����
            StartCoroutine(pistoltime());

            //�����ִ� bullet ����ŭ ǥ��
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
        //�Ѿ��� �����ִٸ�
        if (shotgunBullet > 0)
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
                if (bullet != null)
                {
                    // ���� ���� �����
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

            //�Ѿ� ���� -1
            shotgunBullet--;

            shotgunTerm = false;
            //���� �ð� �� shotgunterm�� true�� �ٲ��ִ� �ڷ�ƾ ����
            StartCoroutine(shotguntime());

            //�����ִ� bullet ����ŭ ǥ��
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
        //�Ѿ��� �����ִٸ�
        if (SMGBullet > 0)
        {
            //�Ѿ� ������ƮǮ��
            GameObject bullet = ObjectPooler.SharedInstance.GetPooledObject();
            if (bullet != null)
            {
                Bullet B = bullet.GetComponent<Bullet>();
                B.active = true;
                B.identity = identity;
                B.gunNum = 3;
                bullet.GetComponent<Bullet>().startPosition = startPosition + rotate;
                bullet.SetActive(true); // activate it
                //�Ѿ� ���� ��ġ
                bullet.transform.position = startPosition + rotate;
                bullet.transform.rotation = Quaternion.LookRotation(rotate);

                // Rigidbody�� �ӵ� �ֱ�
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = rotate * SMGBulletSpeed;
                }
            }
            //�Ѿ� ���� -1
            SMGBullet--;

            SMGTerm = false;
            //���� �ð� �� SNGterm�� true�� �ٲ��ִ� �ڷ�ƾ ����
            StartCoroutine(SMGtime());

            //�����ִ� bullet ����ŭ ǥ��
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
        //���õ� �ѿ� ���� �ʱ�ȭ�ϰ� �̹����� �ٽ� ����
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
        // 0.5�� �� ��߻� ����
        yield return new WaitForSeconds(0.5f);
        pistolTerm = true;
    }
    IEnumerator shotguntime()
    {
        // 1�� �� ��߻� ����
        yield return new WaitForSeconds(1f);
        shotgunTerm = true;
    }
    IEnumerator SMGtime()
    {
        // 0.1�� �� ��߻� ����
        yield return new WaitForSeconds(0.1f);
        SMGTerm = true;
    }

    IEnumerator PistolReload()
    {
        reloading = true;
        reloadingText.enabled = true;
        //������ �ð�
        yield return new WaitForSeconds(1f);

        pistolBullet = 6;

        //�����ִ� bullet ����ŭ ǥ��
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
        //������ �ð�
        yield return new WaitForSeconds(1f);

        shotgunBullet = 6;

        //�����ִ� bullet ����ŭ ǥ��
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
        //������ �ð�
        yield return new WaitForSeconds(1f);

        SMGBullet = 30;

        //�����ִ� bullet ����ŭ ǥ��
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
