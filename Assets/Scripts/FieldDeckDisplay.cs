
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FieldDeckDisplay : NetworkBehaviour
{
    public float cardSpacing = 1.5f; // 카드 간격
    public Vector3 startPosition = Vector3.zero; // 필드 덱 시작 위치

    public void DisplayFieldDeck(List<Card> fieldDeckCards)
    {
        // 기존에 표시되던 카드들을 모두 제거 (새로 정렬하기 위함)
        // 필드 덱은 카드가 교체되거나 리필될 때마다 새로 그려야 하므로 이 로직은 유지합니다.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        for (int i = 0; i < fieldDeckCards.Count; i++)
        {
            Card card = fieldDeckCards[i];
            if (card != null && card.gameObject != null)
            {
                card.gameObject.SetActive(true); // 카드 활성화
                // 카드의 부모를 FieldDeckDisplay 오브젝트로 설정
                card.NetworkObject.TrySetParent(this.NetworkObject);

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
