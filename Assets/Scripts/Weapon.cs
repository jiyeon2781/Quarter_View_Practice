using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,  Range }; // ���� Ÿ��
    public Type type; 
    public int damage; // ������
    public float rate; // ���ݼӵ�
    public int maxAmmo; // ��ü ź��
    public int curAmmo; // ���� ź��

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPos; // ������ ���� ��ġ, �߻� ��ġ
    public GameObject bullet; // ������ ����, �Ѿ�
    public Transform bulletCasePos; // ź�� ��ġ
    public GameObject bulletCase; // ź��

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
            StartCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
        }
        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing() // ������ �Լ� Ŭ����
    {
        // 1
        yield return new WaitForSeconds(0.1f); // ����� �����ϴ� Ű���� : yield / 0.1�� ���
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
        // 1. �Ѿ� �߻�
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        // �Ѿ� �ν��Ͻ�ȭ(�Ѿ� ����)
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50; // �ν��Ͻ�ȭ�� �Ѿ˿� �ӵ� ����
        yield return null;

        // 2. ź�� ����
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        // ź�� �ν��Ͻ�ȭ(ź�� ����)
        Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();
        // ƨ����� ���� ���ϴ� ��������
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3); 
        caseRigid.AddForce(caseVec, ForceMode.Impulse);
        caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        // �ν��Ͻ�ȭ�� ź�ǿ� ������ �� ���ϱ�
    }

    // �Ϲ� �Լ� : Use() ���� ��ƾ -> Swing() ���� ��ƾ -> Use() ���� ��ƾ (���� ����)
    // �ڷ�ƾ �Լ� : Use() ���� ��ƾ + Swing() �ڷ�ƾ(Co-Op) (���� ����)
}
