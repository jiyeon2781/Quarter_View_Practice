using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target; // 공전 목표
    public float orbitSpeed; // 공전 속도
    Vector3 offset; // 목표와의 거리

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime); // 타겟 주위를 회전하는 함수
        // 목표가 움직이면 일그러지는 단점 존재
        // RotateAround() 후의 위치를 가지고 목표와 거리를 유지
        offset = transform.position - target.position;
    }
}
