using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    public GameObject[] itemObject;
    public int[] itemPrice;
    public Transform[] itemPos;

    public string[] talkData;
    public Text talkText;

    Player enterPlayer;
    public void Enter(Player player) // 입장, 입장 시 플레이어 정보를 저장하면서 UI 위치 이동
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

    public void Exit() // 퇴장
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if (price > enterPlayer.coin)
        {
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }

        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3,3) + Vector3.forward * Random.Range(-3,3);
        Instantiate(itemObject[index], itemPos[index].position + ranVec, itemPos[index].rotation);
        // 구입 성공시 아이템 생성
    }
    IEnumerator Talk()
    {
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2f);
    }
}
