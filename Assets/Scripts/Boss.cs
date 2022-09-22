using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;
    public bool isLook; // 플레이어를 바라보는 플래그

    Vector3 lookVec;
    Vector3 tauntVec;
    
    void Awake() // Awake는 자식 스크립트만 단독 실행됨 (Enemy에 있는걸 사용하기 위해서는 Enemy의 Awake를 Start로 변경해야함
        // Start로 바꾸면 영향을 받는 함수들이 많으므로 자식 스크립트에 붙여넣기 함
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        // material은 Mesh Renderer 컴포넌트에서 접근가능
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            StopAllCoroutines();
            transform.position = Vector3.zero;
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else nav.SetDestination(tauntVec);
    }

    IEnumerator Think() // 행동 패턴을 결정해준는 코루틴 생성
    {
        yield return new WaitForSeconds(0.1f);
        int ranAction = Random.Range(0, 5); // 행동 패턴을 만들기 위해 호출 (0~4);
        Debug.Log(ranAction);
        switch(ranAction)
        {
            case 0:
            case 1:
                // 미사일 발사 패턴
                StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                StartCoroutine(RockShot());
                // 돌 굴러가는 패턴
                break;
            case 4:
                StartCoroutine(Taunt());
                // 점프 공격 패턴
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        // 미사일 생성
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        // 미사일 생성
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target;
        yield return new WaitForSeconds(2f);


        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false; // 기를 모을때 바라보는 것은 잠시 정지
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);
        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        isLook = false;
        // 콜라이더가 플레이어를 밀지 않도록 비활성
        nav.isStopped = false;
        boxCollider.enabled = false;
        anim.SetTrigger("doTaunt");
        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    }
}
