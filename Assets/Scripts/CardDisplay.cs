
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TextMeshPro rankText;
    public TextMeshPro suitText;

    public void SetCard(Card card)
    {
        if (card == null)
        {
            rankText.text = "";
            suitText.text = "";
            return;
        }

        rankText.text = GetRankString(card.cardRank);
        suitText.text = GetSuitString(card.cardSuit);

        // 색상 변경 (선택 사항: 스페이드/클럽은 검정, 하트/다이아는 빨강)
        if (card.cardSuit == Card.Suit.Heart || card.cardSuit == Card.Suit.Diamond)
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
