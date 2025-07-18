
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FieldDeckDisplay : NetworkBehaviour
{
    public float cardSpacing = 1.5f; // 카드 간격
    public Vector3 startPosition = Vector3.zero; // 필드 덱 시작 위치

    public readonly NetworkList<ulong> fieldDeckCardIds = new NetworkList<ulong>();
    private List<GameObject> displayedCardObjects = new List<GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            fieldDeckCardIds.OnListChanged += HandleFieldDeckChanged;
            DisplayFieldDeck();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            fieldDeckCardIds.OnListChanged -= HandleFieldDeckChanged;
        }
    }

    private void HandleFieldDeckChanged(NetworkListEvent<ulong> changeEvent)
    {
        DisplayFieldDeck();
    }

    public void DisplayFieldDeck()
    {
        // 이전에 표시된 카드들을 모두 비활성화합니다.
        foreach (GameObject cardObject in displayedCardObjects)
        {
            if (cardObject != null) // 오브젝트가 다른 곳에서 파괴되었을 수 있으므로 확인
            {
                cardObject.SetActive(false);
            }
        }
        displayedCardObjects.Clear();

        for (int i = 0; i < fieldDeckCardIds.Count; i++)
        {
            ulong cardId = fieldDeckCardIds[i];
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
            {
                Card card = networkObject.GetComponent<Card>();
                if (card != null && card.gameObject != null)
                {
                    GameObject cardGo = card.gameObject;
                    cardGo.SetActive(true); // 카드 활성화
                    cardGo.transform.SetParent(this.transform); // 부모 설정

                    // 카드 위치 설정
                    Vector3 newPosition = startPosition + new Vector3(i * cardSpacing, 0, 0);
                    // cardGo.transform.localPosition = newPosition; // 즉시 이동 대신 애니메이션 호출
                    card.MoveTo(transform.TransformPoint(newPosition), 0.3f);
                    cardGo.transform.localRotation = Quaternion.identity;
                    cardGo.transform.localScale = new Vector3(0.7f, 1f, 1f);

                    // 카드의 선택 상태를 리셋하여 현재 위치를 기준으로 다시 선택 효과가 적용되도록 합니다.
                    card.GetComponent<CardDisplay>()?.ResetSelectionState();

                    displayedCardObjects.Add(cardGo); // 표시된 카드 리스트에 추가
                }
            }
        }
    }
}
