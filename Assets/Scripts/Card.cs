
using UnityEngine;
using Unity.Netcode; // NetworkBehaviour를 위해 추가

public class Card : NetworkBehaviour
{
    public enum Suit { Spade, Heart, Diamond, Club }
    public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public NetworkVariable<Suit> cardSuit = new NetworkVariable<Suit>();
    public NetworkVariable<Rank> cardRank = new NetworkVariable<Rank>();
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(0); // 이 카드를 소유한 플레이어의 ClientId
    public NetworkVariable<bool> isSelected = new NetworkVariable<bool>(false); // 카드의 선택 상태

    private GameManager gameManager;
    private CardDisplay cardDisplay;

    public override void OnNetworkSpawn()
    {
        gameManager = FindObjectOfType<GameManager>();
        cardDisplay = GetComponent<CardDisplay>();

        // CardDisplay 컴포넌트 설정
        if (cardDisplay != null)
        {
            // NetworkVariable의 Value를 사용하여 CardDisplay를 설정합니다.
            cardDisplay.SetCard(this);
        }

        // isSelected 변수의 값이 변경될 때마다 OnSelectedChanged 메서드를 호출하도록 구독합니다.
        isSelected.OnValueChanged += OnSelectedChanged;
        // 초기 상태를 반영합니다.
        OnSelectedChanged(false, isSelected.Value);
    }

    public override void OnNetworkDespawn()
    {
        // 구독을 해제합니다.
        isSelected.OnValueChanged -= OnSelectedChanged;
    }

    private void OnSelectedChanged(bool previousValue, bool newValue)
    {
        if (cardDisplay != null)
        {
            cardDisplay.SetSelected(newValue);
        }
    }
}
