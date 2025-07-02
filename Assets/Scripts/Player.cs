using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public List<Card> hand = new List<Card>();
    public string playerName;

    public enum PokerHandRank
    {
        HighCard = 0,
        Pair = 1,
        TwoPair = 2,
        ThreeOfAKind = 3,
        Straight = 4,
        Flush = 5,
        FullHouse = 6,
        FourOfAKind = 7,
        StraightFlush = 8,
        RoyalFlush = 9
    }

    public void AddCardToHand(Card card)
    {
        hand.Add(card);
        Debug.Log(playerName + " received " + card.cardSuit + " " + card.cardRank);
    }

    public void ClearHand()
    {
        hand.Clear();
        Debug.Log(playerName + "'s hand cleared.");
    }

    public PokerHandRank EvaluateHand()
    {
        if (hand.Count != 5) // 포커 족보는 5장 기준으로 평가
        {
            Debug.LogWarning("Hand must contain 5 cards for poker evaluation.");
            return PokerHandRank.HighCard;
        }

        // 카드 정렬 (족보 평가를 위해)
        List<int> ranks = hand.Select(card => (int)card.cardRank).OrderBy(r => r).ToList();
        List<Card.Suit> suits = hand.Select(card => card.cardSuit).ToList();

        bool isFlush = suits.Distinct().Count() == 1;
        bool isStraight = IsStraight(ranks);

        // 랭크별 카운트
        var rankCounts = ranks.GroupBy(r => r).Select(g => new { Rank = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).ThenByDescending(x => x.Rank).ToList();

        // 족보 판별
        if (isStraight && isFlush)
        {
            if (ranks.SequenceEqual(new List<int> { 1, 10, 11, 12, 13 })) // Royal Flush (A, K, Q, J, 10)
            {
                return PokerHandRank.RoyalFlush;
            }
            return PokerHandRank.StraightFlush;
        }
        else if (rankCounts[0].Count == 4)
        {
            return PokerHandRank.FourOfAKind;
        }
        else if (rankCounts[0].Count == 3 && rankCounts[1].Count == 2)
        {
            return PokerHandRank.FullHouse;
        }
        else if (isFlush)
        {
            return PokerHandRank.Flush;
        }
        else if (isStraight)
        {
            return PokerHandRank.Straight;
        }
        else if (rankCounts[0].Count == 3)
        {
            return PokerHandRank.ThreeOfAKind;
        }
        else if (rankCounts[0].Count == 2 && rankCounts[1].Count == 2)
        {
            return PokerHandRank.TwoPair;
        }
        else if (rankCounts[0].Count == 2)
        {
            return PokerHandRank.Pair;
        }
        else
        {
            return PokerHandRank.HighCard;
        }
    }

    private bool IsStraight(List<int> ranks)
    {
        // Ace can be high (13) or low (1)
        if (ranks.SequenceEqual(new List<int> { 1, 2, 3, 4, 5 })) return true; // A,2,3,4,5
        if (ranks.SequenceEqual(new List<int> { 10, 11, 12, 13, 1 })) return true; // 10,J,Q,K,A (Ace as 14)

        for (int i = 0; i < ranks.Count - 1; i++)
        {
            if (ranks[i+1] - ranks[i] != 1)
            {
                return false;
            }
        }
        return true;
    }
}