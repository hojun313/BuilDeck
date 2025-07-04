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
        if (count > 0)
        {
            if (displayObject == null)
            {
                // 카드 프리팹을 사용하여 뒷면 카드 생성
                displayObject = Instantiate(cardPrefab, positionTransform.position, Quaternion.identity, positionTransform);
                // 카드를 뒷면으로 보이게 회전 (필요하다면)
                displayObject.transform.localRotation = Quaternion.Euler(0, 180, 0); // Y축 180도 회전
            }
            // 스택처럼 보이게 약간의 Z 오프셋 적용
            displayObject.transform.localPosition = new Vector3(0, 0, -count * cardStackOffset);
            displayObject.SetActive(true);
        }
        else
        {
            if (displayObject != null)
            {
                displayObject.SetActive(false);
            }
        }

        // 현재 표시 객체 업데이트 (메인 덱 또는 죽은 카드 더미)
        if (positionTransform == mainDeckPosition)
        {
            currentMainDeckDisplay = displayObject;
        }
        else if (positionTransform == discardPilePosition)
        {
            currentDiscardPileDisplay = displayObject;
        }
    }
}
