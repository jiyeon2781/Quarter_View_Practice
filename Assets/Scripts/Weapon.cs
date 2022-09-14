using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,  Range }; // ���� Ÿ��
    public Type type; 
    public int damage; // ������
    public float rate; // ���ݼӵ�
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
            StartCoroutine("Swing"); // �ڷ�ƾ ���� �Լ�
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

    // �Ϲ� �Լ� : Use() ���� ��ƾ -> Swing() ���� ��ƾ -> Use() ���� ��ƾ (���� ����)
    // �ڷ�ƾ �Լ� : Use() ���� ��ƾ + Swing() �ڷ�ƾ(Co-Op) (���� ����)
}
