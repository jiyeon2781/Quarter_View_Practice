using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase; // 추적 결정

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material; 
        // material은 Mesh Renderer 컴포넌트에서 접근가능
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        Invoke("ChaseStart",2);
    }
    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if (isChase)  nav.SetDestination(target.position);
    }
    void FreezeVelocity() 
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // 물리 회전 속도
        }
    }

    void FixedUpdate()
    {
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
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            mat.color = Color.white;
        }
        else
        {
            mat.color = Color.gray;
            gameObject.layer = 12; // 레이어 번호 그대로 적용
            isChase = false;
            nav.enabled = false; // 사망 리액션을 유지하기 위해 NavAgent 비활성
            anim.SetTrigger("doDie");

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
