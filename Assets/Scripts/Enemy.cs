using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D }; // 적 타입
    public Type enemyType; // 적 타입을 저장할 변수
    public int maxHealth;
    public int curHealth;
    public int score; // 적 점수
    public GameManager manager;

    public Transform target;
    public BoxCollider meleeArea; // 콜라이더 담을 변수 추가
    public GameObject bullet; // C타입에서 사용할 오브젝트
    public GameObject[] coins; // 동전
    public bool isChase; // 추적 결정
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs; // 피격 이펙트 플레이어처럼 모든 메테리얼로 변경
    public NavMeshAgent nav;
    public Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>(); 
        // material은 Mesh Renderer 컴포넌트에서 접근가능
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if (enemyType != Type.D) Invoke("ChaseStart",2);
    }
    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }


    void FreezeVelocity() 
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // 물리 회전 속도
        }
    }

    void Targeting()
    {
        if (!isDead && enemyType != Type.D)
        {
            float targetRadius = 0f; // 공격 두께
            float targetRange = 0f; // 공격 범위

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius,
                transform.forward, targetRange, LayerMask.GetMask("Player"));
            if (rayHits.Length > 0 && !isAttack) // 플레이어가 잡히고, 공격을 하지 않을때 공격 코루틴 실행
            {
                StartCoroutine(Attack());
            }
        }
        
    }

    IEnumerator Attack()
    {
        isChase = false; // 정지 후 애니메이션과 함께 공격범위 활성화
        isAttack = true;
        anim.SetBool("isAttack",true);
        
        switch(enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse); // 돌격 구현
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation); // 미사일 인스턴스화
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        


        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee") // 근접 공격
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position; // 현재 위치에 피격 위치를 빼 반작용 방향 구하기
            
            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullet") // 원거리 공격
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position; // 현재 위치에 피격 위치를 빼 반작용 방향 구하기
            Destroy(other.gameObject); // 총알의 경우, 적과 닿았을 때 삭제되도록 Destroy() 호출
            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        // 피격함수로직은 이전시간 구현과 동일(위 코드 참고)
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec , bool isGrenade) 
        // 로직 담을 코루틴 생성, 수류탄의 리액션을 위해 bool 매개변수 추가
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.grey;
            gameObject.layer = 12; // 레이어 번호 그대로 적용
            isDead = true;
            isChase = false;
            nav.enabled = false; // 사망 리액션을 유지하기 위해 NavAgent 비활성
            anim.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    break;
                case Type.B:
                    manager.enemyCntB--;
                    break;
                case Type.C:
                    manager.enemyCntC--;
                    break;
                case Type.D:
                    manager.enemyCntD--;
                    break;
            }

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); // 넉백 구현
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); // 넉백 구현
            }

            Destroy(gameObject, 4);
        }
    }
}
