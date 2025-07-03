
using UnityEngine;
using Unity.Netcode; // NetworkBehaviour를 위해 추가

public class Card : NetworkBehaviour
{
    public enum Suit { Spade, Heart, Diamond, Club }
    public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public NetworkVariable<Suit> cardSuit = new NetworkVariable<Suit>();
    public NetworkVariable<Rank> cardRank = new NetworkVariable<Rank>();
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(0); // 이 카드를 소유한 플레이어의 ClientId

    private GameManager gameManager;

    public override void OnNetworkSpawn()
    {
        gameManager = FindObjectOfType<GameManager>();

        // CardDisplay 컴포넌트 설정
        CardDisplay cardDisplay = GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            // NetworkVariable의 Value를 사용하여 CardDisplay를 설정합니다.
            cardDisplay.SetCard(this);
        }
    }

    void OnMouseDown()
    {
        // 서버이거나, 클라이언트이면서 자신의 소유인 경우에만 클릭 처리
        if (IsServer || (IsClient && IsOwner))
        {
            if (gameManager != null)
            {
                gameManager.HandleCardSelection(this);
            }
        }
    }
}
