using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TextMeshPro rankText;
    public TextMeshPro suitText;

    private Vector3 _currentUnselectedScale;
    private Vector3 _currentBaseLocalPosition;
    private bool _wasSelected = false;
    private const float selectedScaleMultiplier = 1.2f;
    private const float selectedYOffset = 0.5f;

    void Awake()
    {
        _currentUnselectedScale = transform.localScale;
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            if (!_wasSelected)
            {
                _currentUnselectedScale = transform.localScale;
                _currentBaseLocalPosition = transform.localPosition;
                _wasSelected = true;
            }
            transform.localScale = _currentUnselectedScale * selectedScaleMultiplier;
            transform.localPosition = _currentBaseLocalPosition + new Vector3(0, selectedYOffset, 0);
        }
        else
        {
            if (_wasSelected)
            {
                transform.localScale = _currentUnselectedScale;
                transform.localPosition = _currentBaseLocalPosition;
            }
        }
    }

    public void SetCard(Card card)
    {
        if (card == null)
        {
            rankText.text = "";
            suitText.text = "";
            return;
        }

        rankText.text = GetRankString(card.cardRank.Value);
        suitText.text = GetSuitString(card.cardSuit.Value);

        if (card.cardSuit.Value == Card.Suit.Heart || card.cardSuit.Value == Card.Suit.Diamond)
        {
            rankText.color = Color.red;
            suitText.color = Color.red;
        }
        else
        {
            rankText.color = Color.black;
            suitText.color = Color.black;
        }
    }

    private string GetRankString(Card.Rank rank)
    {
        switch (rank)
        {
            case Card.Rank.Ace: return "A";
            case Card.Rank.Jack: return "J";
            case Card.Rank.Queen: return "Q";
            case Card.Rank.King: return "K";
            default: return ((int)rank).ToString();
        }
    }

    private string GetSuitString(Card.Suit suit)
    {
        switch (suit)
        {
            case Card.Suit.Spade: return "♠";
            case Card.Suit.Heart: return "♥";
            case Card.Suit.Diamond: return "♦";
            case Card.Suit.Club: return "♣";
            default: return "?";
        }
    }

    public void ResetSelectionState()
    {
        // 카드가 선택된 적이 없던 상태로 되돌려, 다음 선택 시 위치를 새로 저장하도록 합니다.
        _wasSelected = false;
    }

    void OnMouseDown()
    {
        if (FindObjectOfType<GameManager>() == null || !Unity.Netcode.NetworkManager.Singleton.IsClient) return;

        Unity.Netcode.NetworkObject playerObject = Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject != null)
        {
            Player localPlayer = playerObject.GetComponent<Player>();
            if (localPlayer != null)
            {
                localPlayer.RequestSelectCard(GetComponent<Card>());
            }
        }
    }
}