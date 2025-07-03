
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TextMeshPro rankText;
    public TextMeshPro suitText;

    private Vector3 _currentUnselectedScale; // 카드가 선택되지 않았을 때의 현재 스케일을 저장합니다.
    private Vector3 _currentBaseLocalPosition; // 카드가 선택되기 전의 기본 위치를 저장합니다.
    private const float selectedScaleMultiplier = 1.2f;
    private const float selectedYOffset = 0.5f; // 선택 시 Y축으로 이동할 거리

    void Awake()
    {
        // Awake 시점의 스케일을 초기 unselected 스케일로 설정합니다.
        _currentUnselectedScale = transform.localScale;
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            // 선택될 때 현재 스케일과 위치를 저장합니다.
            _currentUnselectedScale = transform.localScale;
            _currentBaseLocalPosition = transform.localPosition;

            transform.localScale = _currentUnselectedScale * selectedScaleMultiplier;
            transform.localPosition = _currentBaseLocalPosition + new Vector3(0, selectedYOffset, 0);
        }
        else
        {
            // 저장된 스케일과 위치로 돌아갑니다.
            transform.localScale = _currentUnselectedScale;
            transform.localPosition = _currentBaseLocalPosition;
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

        // 색상 변경 (선택 사항: 스페이드/클럽은 검정, 하트/다이아는 빨강)
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
}
