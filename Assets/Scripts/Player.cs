using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades; // 공전하는 물체를 컨트롤하기 위해 배열변수 생성
    public int hasGrenades;
    public Camera followCamera; // 플레이어에 메인 카메라 변수 생성

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
    bool interDown; // 상호작용
    bool fireDown; // 공격 키 입력
    bool reloadDown; // 재장전 변수

    bool sDown1;
    bool sDown2;
    bool sDown3;
    // 장비 단축키 1, 2, 3

    bool isJump;
    bool isDodge;
    bool isSwap; // 무기 교체 시간차를 위한 플래그
    bool isReload;
    bool isFireReady = true; // 공격 준비 완료

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid; // 물리효과를 위한 선언
    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    float fireDelay; // 공격 딜레이

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
        Reload();
        Dodge();
        Swap();
        Interaction();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); // Axis 값을 정수로 반환하는 함수
        vAxis = Input.GetAxisRaw("Vertical");
        walkDown = Input.GetButton("Walk"); // 누를때만 작동되도록 함수 사용
        jumpDown = Input.GetButtonDown("Jump");
        fireDown = Input.GetButton("Fire1");
        reloadDown = Input.GetButtonDown("Reload");
        interDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized; //normalized : 방향값이 1로 보정된 벡터

        if (isDodge) moveVec = dodgeVec;
        // 회피 중에는 움직임 벡터 -> 회피방향 벡터로 바뀌도록 구현
        if (isSwap || !isFireReady || isReload) moveVec = Vector3.zero;

        transform.position += moveVec * speed * (walkDown ? 0.3f : 1f) * Time.deltaTime; // 이동은 꼭 deltaTime 추가

        anim.SetBool("IsRun", moveVec != Vector3.zero); // 파라미터 값 설정
        anim.SetBool("IsWalk", walkDown);
    }

    void Turn()
    {
        // 1. 키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        // 2. 마우스에 의한 회전
        if (fireDown)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); // 스크린에서 월드로 Ray를 쏘는 함수
            RaycastHit rayHit; // RaycastHit 정보를 저장할 변수
            if (Physics.Raycast(ray, out rayHit, 100)) // out : return 처럼 반환값을 주어진 변수에 저장하는 키워드
            {
                // RayCaseHit의 마우스 클릭 위치를 활용해 회전을 구현
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0; // RayCastHit의 높이는 무시하도록 y축 값을 0으로 초기화
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jumpDown && moveVec == Vector3.zero && !isJump && !isDodge)
        {
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse); // 물리적인 힘을 가하는 함수
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true; // 무한 점프를 막기 위해 제약 조건 필요
        }
    }

    void Attack()
    {
        if (equipWeapon == null) return; // 무기가 없을때 종료
        fireDelay += Time.deltaTime; // 공격 딜레이에 시간을 더해주고 공격 가능 여부 확인
        isFireReady = equipWeapon.rate < fireDelay;

        if (fireDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0; // 공격 딜레이를 0으로 돌려 다음 공격까지 기다리도록 작성
        }
    }

    void Reload()
    {
        if (equipWeapon == null) return; //손에 들린 무기가 없을 때
        if (equipWeapon.type == Weapon.Type.Melee) return; // 근접무기 일때
        if (ammo == 0) return; // 플레이어의 총알이 1개도 없을 때

        if (reloadDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 3f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;// 플레이어가 소지한 탄을 고려해서 계산
        equipWeapon.curAmmo = reAmmo; // 무기에 탄이 들어감
        ammo -= reAmmo; // 들어간 개수만큼 플레이어가 소지한 탄을 빼줌
        isReload = false;
    }

    void Dodge()
    {
        if (jumpDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2; // 회피는 이동속도 2배로 상승하도록 설정
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f); // 시간차 함수
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
            if (equipWeapon != null) equipWeapon.gameObject.SetActive(false); // 빈손일때 실행 X
            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.5f); // false를 주기 위한 시간차 함수
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
                hasWeapons[weaponIndex] = true; // 아이템 정보를 가져와 해당 무기 입수 체크

                Destroy(nearObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision) // 점프 후 착지 구현
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other) // 아이템 상호작용 코드
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
