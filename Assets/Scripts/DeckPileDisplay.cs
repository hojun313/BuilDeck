using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class DeckPileDisplay : NetworkBehaviour
{
    public GameObject cardPrefab; // 카드 프리팹
    public Transform mainDeckPosition; // 메인 덱이 표시될 위치
    public Transform discardPilePosition; // 죽은 카드 더미가 표시될 위치
    public float cardStackOffset = 0.01f; // 카드 스택의 z-오프셋

    private GameObject currentMainDeckDisplay;
    private GameObject currentDiscardPileDisplay;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // GameManager의 NetworkVariable 변경 이벤트 구독
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.deckCount.OnValueChanged += OnMainDeckCountChanged;
                gameManager.discardPileCount.OnValueChanged += OnDiscardPileCountChanged;

                // 초기 상태 업데이트
                OnMainDeckCountChanged(0, gameManager.deckCount.Value);
                OnDiscardPileCountChanged(0, gameManager.discardPileCount.Value);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.deckCount.OnValueChanged -= OnMainDeckCountChanged;
                gameManager.discardPileCount.OnValueChanged -= OnDiscardPileCountChanged;
            }
        }
    }

    private void OnMainDeckCountChanged(int previousValue, int newValue)
    {
        UpdateDeckDisplay(currentMainDeckDisplay, mainDeckPosition, newValue);
    }

    private void OnDiscardPileCountChanged(int previousValue, int newValue)
    {
        UpdateDeckDisplay(currentDiscardPileDisplay, discardPilePosition, newValue);
    }

    private void UpdateDeckDisplay(GameObject displayObject, Transform positionTransform, int count)
    {
        // 이전에 생성된 모든 카드 뒷면 시각화 오브젝트를 파괴합니다.
        foreach (Transform child in positionTransform)
        {
            Destroy(child.gameObject);
        }

        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject cardBackInstance = Instantiate(cardBackVisualPrefab, positionTransform.position, Quaternion.identity, positionTransform);
                // 카드를 뒷면으로 보이게 회전 (필요하다면)
                cardBackInstance.transform.localRotation = Quaternion.Euler(0, 180, 0); // Y축 180도 회전
                // 스택처럼 보이게 약간의 Z 오프셋 적용
                cardBackInstance.transform.localPosition = new Vector3(0, 0, -i * cardStackOffset);
                cardBackInstance.SetActive(true);
            }
        }
    }
}
