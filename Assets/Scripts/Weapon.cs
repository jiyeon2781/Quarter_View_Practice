using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,  Range }; // 무기 타입
    public Type type; 
    public int damage; // 데미지
    public float rate; // 공격속도
    public int maxAmmo; // 전체 탄약
    public int curAmmo; // 현재 탄약

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos; // 프리팹 생성 위치, 발사 위치
    public GameObject bullet; // 프리팹 저장, 총알
    public Transform bulletCasePos; // 탄피 위치
    public GameObject bulletCase; // 탄피

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴 정지 함수
            StartCoroutine("Swing"); // 코루틴 실행 함수
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing() // 열거형 함수 클래스
    {
        // 1
        yield return new WaitForSeconds(0.1f); // 결과를 전달하는 키워드 : yield / 0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        // 2
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
        //3
        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        // 1. 총알 발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        // 총알 인스턴스화(총알 생성)
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; // 인스턴스화된 총알에 속도 적용
        yield return null;

        // 2. 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        // 탄피 인스턴스화(탄피 생성)
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        // 튕기듯이 힘을 가하는 느낌으로
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3); 
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        // 인스턴스화된 탄피에 랜덤한 힘 가하기
    }

    // 일반 함수 : Use() 메인 루틴 -> Swing() 서브 루틴 -> Use() 메인 루틴 (교차 실행)
    // 코루틴 함수 : Use() 메인 루틴 + Swing() 코루틴(Co-Op) (동시 실행)
}
