
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class FieldDeckDisplay : NetworkBehaviour
{
    public float cardSpacing = 1.5f; // 카드 간격
    public Vector3 startPosition = Vector3.zero; // 필드 덱 시작 위치

    // NetworkList를 사용하여 필드 덱의 카드들의 NetworkObjectId를 동기화합니다.
    public readonly NetworkList<ulong> fieldDeckCardIds = new NetworkList<ulong>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            // 클라이언트에서 NetworkList의 변경 이벤트를 구독합니다.
            fieldDeckCardIds.OnListChanged += HandleFieldDeckChanged;
            // 초기 필드 덱을 표시합니다.
            DisplayFieldDeck();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            // 오브젝트가 사라질 때 이벤트 구독을 해제합니다.
            fieldDeckCardIds.OnListChanged -= HandleFieldDeckChanged;
        }
    }

    private void HandleFieldDeckChanged(NetworkListEvent<ulong> changeEvent)
    {
        // NetworkList가 변경될 때마다 필드 덱을 다시 그립니다.
        DisplayFieldDeck();
    }

    public void DisplayFieldDeck()
    {
        // 기존에 표시되던 카드들을 모두 비활성화합니다.
        // NetworkObject는 서버에서 관리하므로 클라이언트에서 직접 Destroy하지 않습니다.
        // 대신, DisplayFieldDeck에서 활성화할 카드만 활성화합니다.
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        for (int i = 0; i < fieldDeckCardIds.Count; i++)
        {
            ulong cardId = fieldDeckCardIds[i];
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
            {
                Card card = networkObject.GetComponent<Card>();
                if (card != null && card.gameObject != null)
                {
                    card.gameObject.SetActive(true); // 카드 활성화
                    // 카드의 부모를 FieldDeckDisplay 오브젝트로 설정
                    // NetworkObject.TrySetParent는 서버에서만 호출되어야 합니다.
                    // 클라이언트에서는 이미 부모가 설정되어 있다고 가정합니다.
                    if (IsServer)
                    {
                        card.NetworkObject.TrySetParent(this.NetworkObject);
                    }

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
}
