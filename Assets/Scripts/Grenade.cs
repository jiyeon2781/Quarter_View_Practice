using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject meshObject;
    public GameObject effectObject;
    public Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Explosion());
    }

    // 시간차 폭발을 위해 코루틴 선언
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        // 물리적 속도 모두 zero로 초기화
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        
        // mesh 삭제 및 이펙트 활성화
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15, 
            Vector3.up, 0f, LayerMask.GetMask("Enemy")); // 구체 모양의 레이캐스팅 (모든 오브젝트)
        
        foreach(RaycastHit hitObject in rayHits)
        {
            hitObject.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, 5);
    }
}
