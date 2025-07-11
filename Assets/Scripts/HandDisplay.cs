
using System.Collections.Generic;
using UnityEngine;

public class HandDisplay : MonoBehaviour
{
    public float cardSpacing = 0.5f; // 카드의 겹치는 정도를 조절합니다.
    public Vector3 startPosition = Vector3.zero;
    public float arcRadius = 4f; // 부채꼴의 반지름
    public float maxArcAngle = 90f; // 부채꼴의 최대 각도
    public float cardRotation = 10f; // 각 카드의 기울기

    public void DisplayHand(List<Card> hand)
    {
        if (hand.Count == 0) return;

        // 먼저 모든 자식 오브젝트(카드)를 비활성화하여 초기화합니다.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        float angleStep = Mathf.Min(cardRotation, maxArcAngle / hand.Count);
        float totalAngle = angleStep * (hand.Count - 1);
        float startAngle = -totalAngle / 2f;

        for (int i = 0; i < hand.Count; i++)
        {
            Card card = hand[i];
            if (card != null && card.gameObject != null)
            {
                // 카드의 부모를 HandDisplay로 설정하고 활성화합니다.
                card.transform.SetParent(this.transform);
                card.gameObject.SetActive(true);

                float angle = startAngle + i * angleStep;
                float radian = angle * Mathf.Deg2Rad;

                // 부채꼴 모양으로 위치 계산
                float x = arcRadius * Mathf.Sin(radian);
                float y = -arcRadius * Mathf.Cos(radian) + arcRadius; // 카드를 위로 올리는 효과

                Vector3 newPosition = startPosition + new Vector3(x, y, i * -0.01f); // z값으로 카드 순서 보장
                // card.transform.localPosition = newPosition; // 즉시 이동 대신 애니메이션 호출
                card.MoveTo(transform.TransformPoint(newPosition), 0.3f);

                // 카드를 중앙을 향해 기울임
                card.transform.localRotation = Quaternion.Euler(0, 0, -angle);
                card.transform.localScale = new Vector3(0.7f, 1f, 1f);

                // 카드의 선택 상태를 리셋하여 현재 위치를 기준으로 다시 선택 효과가 적용되도록 합니다.
                card.GetComponent<CardDisplay>()?.ResetSelectionState();
            }
        }
    }
}
