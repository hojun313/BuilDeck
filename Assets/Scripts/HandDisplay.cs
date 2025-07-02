
using System.Collections.Generic;
using UnityEngine;

public class HandDisplay : MonoBehaviour
{
    public float cardSpacing = 1.5f; // 카드 간격
    public Vector3 startPosition = Vector3.zero; // 핸드 시작 위치

    public void DisplayHand(List<Card> hand)
{
    for (int i = 0; i < hand.Count; i++)
    {
        Card card = hand[i];
        if (card != null && card.gameObject != null)
        {
            // 카드의 부모를 HandDisplay 오브젝트로 설정 (이미 GameManager에서 설정됨)
            // card.transform.SetParent(this.transform);

            // 카드 위치 설정
            Vector3 newPosition = startPosition + new Vector3(i * cardSpacing, 0, 0);
            card.transform.localPosition = newPosition;

            // 카드 회전 및 스케일 초기화 (필요시)
            card.transform.localRotation = Quaternion.identity;
            card.transform.localScale = Vector3.one;
        }
    }
}
}
