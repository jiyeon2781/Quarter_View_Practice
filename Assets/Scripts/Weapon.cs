using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,  Range }; // 무기 타입
    public Type type; 
    public int damage; // 데미지
    public float rate; // 공격속도
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴 정지 함수
            StartCoroutine("Swing"); // 코루틴 실행 함수
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

    // 일반 함수 : Use() 메인 루틴 -> Swing() 서브 루틴 -> Use() 메인 루틴 (교차 실행)
    // 코루틴 함수 : Use() 메인 루틴 + Swing() 코루틴(Co-Op) (동시 실행)
}
