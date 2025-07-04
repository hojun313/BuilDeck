using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class OpponentHandDisplay : NetworkBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform displayPosition; // 상대방 핸드가 표시될 위치
    public float cardSpacing = 0.5f; // 카드 간격

    private List<GameObject> displayedCards = new List<GameObject>();
    private Player targetPlayer; // 이 디스플레이가 보여줄 상대방 플레이어

    public void SetTargetPlayer(Player player)
    {
        if (targetPlayer != null)
        {
            targetPlayer.handCardCount.OnValueChanged -= OnHandCardCountChanged;
        }
        targetPlayer = player;
        if (targetPlayer != null)
        {
            targetPlayer.handCardCount.OnValueChanged += OnHandCardCountChanged;
            UpdateDisplay(targetPlayer.handCardCount.Value);
        }
    }

    private void OnHandCardCountChanged(int previousValue, int newValue)
    {
        UpdateDisplay(newValue);
    }

    private void UpdateDisplay(int count)
    {
        // 기존 카드 제거
        foreach (GameObject cardGo in displayedCards)
        {
            Destroy(cardGo);
        }
        displayedCards.Clear();

        // 새로운 카드 생성 및 배치
        for (int i = 0; i < count; i++)
        {
            GameObject newCardGo = Instantiate(cardPrefab, displayPosition.position, Quaternion.identity, displayPosition);
            newCardGo.transform.localPosition = new Vector3(i * cardSpacing, 0, 0);
            newCardGo.transform.localRotation = Quaternion.Euler(0, 180, 0); // 뒷면으로 보이게 회전
            newCardGo.transform.localScale = Vector3.one; // 스케일 초기화

            // 상대방 카드이므로 CardDisplay 컴포넌트를 비활성화하여 앞면 내용이 보이지 않도록 합니다.
            CardDisplay cardDisplay = newCardGo.GetComponent<CardDisplay>();
            if (cardDisplay != null)
            {
                cardDisplay.enabled = false;
            }

            displayedCards.Add(newCardGo);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (targetPlayer != null)
        {
            targetPlayer.handCardCount.OnValueChanged -= OnHandCardCountChanged;
        }
        // 씬이 언로드될 때 생성된 카드 오브젝트들을 정리
        foreach (GameObject cardGo in displayedCards)
        {
            Destroy(cardGo);
        }
        displayedCards.Clear();
    }
}
