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

    bool sDown1;
    bool sDown2;
    bool sDown3;
    // ��� ����Ű 1, 2, 3

    bool isJump;
    bool isDodge;
    bool isSwap; // ���� ��ü �ð����� ���� �÷���
    bool isFireReady = true; // ���� �غ� �Ϸ�

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid; // ����ȿ���� ���� ����
    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay; // ���� ������

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rigid = GetComponent<Rigidbody>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
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
        fireDown = Input.GetButtonDown("Fire1");
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
        if (isSwap || !isFireReady) moveVec = Vector3.zero;

        transform.position += moveVec * speed * (walkDown ? 0.3f : 1f) * Time.deltaTime; // �̵��� �� deltaTime �߰�

        anim.SetBool("IsRun", moveVec != Vector3.zero); // �Ķ���� �� ����
        anim.SetBool("IsWalk", walkDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
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

    void Attack()
    {
        if (equipWeapon == null) return; // ���Ⱑ ������ ����
        fireDelay += Time.deltaTime; // ���� �����̿� �ð��� �����ְ� ���� ���� ���� Ȯ��
        isFireReady = equipWeapon.rate < fireDelay;

        if (fireDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger("doSwing");
            fireDelay = 0; // ���� �����̸� 0���� ���� ���� ���ݱ��� ��ٸ����� �ۼ�
        } 
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
