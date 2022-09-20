using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase; // ���� ����

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
        // material�� Mesh Renderer ������Ʈ���� ���ٰ���
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
            rigid.angularVelocity = Vector3.zero; // ���� ȸ�� �ӵ�
        }
    }

    void FixedUpdate()
    {
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee") // ���� ����
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position; // ���� ��ġ�� �ǰ� ��ġ�� �� ���ۿ� ���� ���ϱ�
            
            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullet") // ���Ÿ� ����
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position; // ���� ��ġ�� �ǰ� ��ġ�� �� ���ۿ� ���� ���ϱ�
            Destroy(other.gameObject); // �Ѿ��� ���, ���� ����� �� �����ǵ��� Destroy() ȣ��
            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        // �ǰ��Լ������� �����ð� ������ ����(�� �ڵ� ����)
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }

    IEnumerator OnDamage(Vector3 reactVec , bool isGrenade) 
        // ���� ���� �ڷ�ƾ ����, ����ź�� ���׼��� ���� bool �Ű����� �߰�
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
            gameObject.layer = 12; // ���̾� ��ȣ �״�� ����
            isChase = false;
            nav.enabled = false; // ��� ���׼��� �����ϱ� ���� NavAgent ��Ȱ��
            anim.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); // �˹� ����
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); // �˹� ����
            }
            

            Destroy(gameObject, 4);
        }
    }
}
