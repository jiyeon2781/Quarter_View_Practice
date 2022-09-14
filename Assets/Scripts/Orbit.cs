using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target; // ���� ��ǥ
    public float orbitSpeed; // ���� �ӵ�
    Vector3 offset; // ��ǥ���� �Ÿ�

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime); // Ÿ�� ������ ȸ���ϴ� �Լ�
        // ��ǥ�� �����̸� �ϱ׷����� ���� ����
        // RotateAround() ���� ��ġ�� ������ ��ǥ�� �Ÿ��� ����
        offset = transform.position - target.position;
    }
}
