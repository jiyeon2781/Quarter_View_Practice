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

    // �ð��� ������ ���� �ڷ�ƾ ����
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        // ������ �ӵ� ��� zero�� �ʱ�ȭ
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        
        // mesh ���� �� ����Ʈ Ȱ��ȭ
        meshObject.SetActive(false);
        effectObject.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, 15, 
            Vector3.up, 0f, LayerMask.GetMask("Enemy")); // ��ü ����� ����ĳ���� (��� ������Ʈ)
        
        foreach(RaycastHit hitObject in rayHits)
        {
            hitObject.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, 5);
    }
}
