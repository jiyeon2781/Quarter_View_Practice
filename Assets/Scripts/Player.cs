using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades; // �����ϴ� ��ü�� ��Ʈ���ϱ� ���� �迭���� ����
    public int hasGrenades;
    public GameObject grenadeObject; // ����ź ������ ������ ���� �߰�
    public Camera followCamera; // �÷��̾ ���� ī�޶� ���� ����

    public int ammo;
    public int coin;
    public int health;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades;

    public float jumpPower = 15;
    float hAxis;
    float vAxis;

    bool walkDown;
    bool jumpDown;
    bool interDown; // ��ȣ�ۿ�
    bool fireDown; // ���� Ű �Է�
    bool grenadeDown; 
    bool reloadDown; // ������ ����

    bool sDown1;
    bool sDown2;
    bool sDown3;
    // ��� ����Ű 1, 2, 3

    bool isJump;
    bool isDodge;
    bool isSwap; // ���� ��ü �ð����� ���� �÷���
    bool isReload;
    bool isFireReady = true; // ���� �غ� �Ϸ�
    bool isBorder; // �� �浹 �÷���
    bool isDamage; // ����Ÿ���� ���� bool qustn

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid; // ����ȿ���� ���� ����
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay; // ���� ������

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
        meshs = GetComponentsInChildren<MeshRenderer>(); // �������� ������
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interaction();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis ���� ������ ��ȯ�ϴ� �Լ�
        vAxis = Input.GetAxisRaw("Vertical");
        walkDown = Input.GetButton("Walk"); // �������� �۵��ǵ��� �Լ� ���
        jumpDown = Input.GetButtonDown("Jump");
        fireDown = Input.GetButton("Fire1");
        grenadeDown = Input.GetButtonDown("Fire2");
        reloadDown = Input.GetButtonDown("Reload");
        interDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; //normalized : ���Ⱚ�� 1�� ������ ����

        if (isDodge) moveVec = dodgeVec;
        // ȸ�� �߿��� ������ ���� -> ȸ�ǹ��� ���ͷ� �ٲ�� ����
        if (isSwap || !isFireReady || isReload) moveVec = Vector3.zero;

        if (!isBorder) transform.position += moveVec * speed * (walkDown ? 0.3f : 1f) * Time.deltaTime; // �̵��� �� deltaTime �߰�
        // �̵� ���� ���� (���� ���� �ʾ��� ��)

        anim.SetBool("IsRun", moveVec != Vector3.zero); // �Ķ���� �� ����
        anim.SetBool("IsWalk", walkDown);
    }

    void Turn()
    {
        // 1. Ű���忡 ���� ȸ��
        transform.LookAt(transform.position + moveVec);

        // 2. ���콺�� ���� ȸ��
        if (fireDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // ��ũ������ ����� Ray�� ��� �Լ�
            RaycastHit rayHit; // RaycastHit ������ ������ ����
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return ó�� ��ȯ���� �־��� ������ �����ϴ� Ű����
            {
                // RayCaseHit�� ���콺 Ŭ�� ��ġ�� Ȱ���� ȸ���� ����
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // RayCastHit�� ���̴� �����ϵ��� y�� ���� 0���� �ʱ�ȭ
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jumpDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); // �������� ���� ���ϴ� �Լ�
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true; // ���� ������ ���� ���� ���� ���� �ʿ�
        }
    }

    void Grenade()
    {
        if (hasGrenades == 0) return;
        if (grenadeDown && !isReload && !isSwap)
        {

            // ���콺 ��ġ�� �ٷ� ���� �� �ֵ��� RayCast ���
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // ��ũ������ ����� Ray�� ��� �Լ�
            RaycastHit rayHit; // RaycastHit ������ ������ ����
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return ó�� ��ȯ���� �־��� ������ �����ϴ� Ű����
            {
                // RayCaseHit�� ���콺 Ŭ�� ��ġ�� Ȱ���� ȸ���� ����
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10; // RayCastHit�� ���̴� �����ϵ��� y�� ���� 0���� �ʱ�ȭ

                // ������ ����ź�� ������ٵ� Ȱ���� ������ ����
                GameObject instantGrenade = Instantiate(grenadeObject, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
                // ����ź�� ����

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);

            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null) return; // ���Ⱑ ������ ����
        fireDelay += Time.deltaTime; // ���� �����̿� �ð��� �����ְ� ���� ���� ���� Ȯ��
        isFireReady = equipWeapon.rate < fireDelay;

        if (fireDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0; // ���� �����̸� 0���� ���� ���� ���ݱ��� ��ٸ����� �ۼ�
        }
    }

    void Reload()
    {
        if (equipWeapon == null) return; //�տ� �鸰 ���Ⱑ ���� ��
        if (equipWeapon.type == Weapon.Type.Melee) return; // �������� �϶�
        if (ammo == 0) return; // �÷��̾��� �Ѿ��� 1���� ���� ��

        if (reloadDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;// �÷��̾ ������ ź�� ����ؼ� ���
        equipWeapon.curAmmo = reAmmo; // ���⿡ ź�� ��
        ammo -= reAmmo; // �� ������ŭ �÷��̾ ������ ź�� ����
        isReload = false;
    }

    void Dodge()
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2; // ȸ�Ǵ� �̵��ӵ� 2��� ����ϵ��� ����
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f); // �ð��� �Լ�
        }
    }

    void DodgeOut()
    {
        isDodge = false;
        speed *= 0.5f;
    }

    void Swap()
    {

        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0)) return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1)) return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2)) return;

        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false); // ����϶� ���� X
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.5f); // false�� �ֱ� ���� �ð��� �Լ�
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if (interDown && nearObject != null && !isJump  & !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true; // ������ ������ ������ �ش� ���� �Լ� üũ

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision) // ���� �� ���� ����
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other) // ������ ��ȣ�ۿ� �ڵ�
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch(item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo) ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin) coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth) health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxHasGrenades) hasGrenades = maxHasGrenades;

                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>(); // Bullet ��ũ��Ʈ ��Ȱ�� -> ������ ����
                health -= enemyBullet.damage;
                if (other.GetComponent<Rigidbody>() != null) Destroy(other.gameObject);
                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true; // �������°� ��
        foreach(MeshRenderer mesh in meshs)
        {
            // ��� ������ ���� ����
            mesh.material.color = Color.yellow;
        }
        yield return new WaitForSeconds(1f);
        isDamage = false; // ����
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
    }
    

    void FreezeRotation() // �÷��̾� �ڵ� ȸ�� ����
    {
        rigid.angularVelocity = Vector3.zero; // ���� ȸ�� �ӵ�
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green); // Scene ������ Ray�� �����ִ� �Լ�
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall")); // Ray�� ��� ��� ������Ʈ�� �����ϴ� �Լ�

    }

    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon") nearObject = other.gameObject;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon") nearObject = null;
    }
}
