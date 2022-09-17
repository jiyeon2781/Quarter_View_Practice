using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;

    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponent<MeshRenderer>().material; 
        // material은 Mesh Renderer 컴포넌트에서 접근가능
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee") // 근접 공격
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position; // 현재 위치에 피격 위치를 빼 반작용 방향 구하기
            
            StartCoroutine(OnDamage(reactVec));
        }
        else if (other.tag == "Bullet") // 원거리 공격
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position; // 현재 위치에 피격 위치를 빼 반작용 방향 구하기
            Destroy(other.gameObject); // 총알의 경우, 적과 닿았을 때 삭제되도록 Destroy() 호출
            StartCoroutine(OnDamage(reactVec));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec) // 로직 담을 코루틴 생성
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

            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse); // 넉백 구현

            Destroy(gameObject, 4);
        }
    }
}
