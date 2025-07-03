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

    public class HandEvaluationResult
    {
        public PokerHandRank Rank { get; set; }
        public List<int> HighCardRanks { get; set; } = new List<int>();
    }

    public HandEvaluationResult EvaluateHand()
    {
        if (hand.Count != 5)
        {
            Debug.LogWarning("Hand must contain 5 cards for poker evaluation.");
            return new HandEvaluationResult { Rank = PokerHandRank.HighCard };
        }

        // Sort ranks descending, Ace is 1 by default.
        List<int> ranks = hand.Select(card => (int)card.cardRank).OrderByDescending(r => r).ToList();
        List<Card.Suit> suits = hand.Select(card => card.cardSuit).ToList();

        bool isFlush = suits.Distinct().Count() == 1;
        bool isStraight = IsStraight(ranks);

        // For tie-breaking Ace-high straights (and straight flushes), we need to treat Ace as 14.
        List<int> highCardRanks = new List<int>(ranks);
        if (isStraight && ranks.SequenceEqual(new List<int> { 13, 12, 11, 10, 1 }))
        {
            highCardRanks = new List<int> { 14, 13, 12, 11, 10 }; // Remap for correct comparison
        }

        var rankCounts = ranks.GroupBy(r => r)
                              .Select(g => new { Rank = g.Key, Count = g.Count() })
                              .OrderByDescending(x => x.Count)
                              .ThenByDescending(x => x.Rank)
                              .ToList();

        // Evaluation logic
        if (isStraight && isFlush)
        {
            bool isRoyal = highCardRanks.Contains(14); // Check if it was an Ace-high straight
            return new HandEvaluationResult { Rank = isRoyal ? PokerHandRank.RoyalFlush : PokerHandRank.StraightFlush, HighCardRanks = highCardRanks };
        }
        if (rankCounts[0].Count == 4)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.FourOfAKind, HighCardRanks = rankCounts.Select(rc => rc.Rank).ToList() };
        }
        if (rankCounts[0].Count == 3 && rankCounts[1].Count == 2)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.FullHouse, HighCardRanks = rankCounts.Select(rc => rc.Rank).ToList() };
        }
        if (isFlush)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.Flush, HighCardRanks = ranks };
        }
        if (isStraight)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.Straight, HighCardRanks = highCardRanks };
        }
        if (rankCounts[0].Count == 3)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.ThreeOfAKind, HighCardRanks = rankCounts.Select(rc => rc.Rank).ToList() };
        }
        if (rankCounts[0].Count == 2 && rankCounts[1].Count == 2)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.TwoPair, HighCardRanks = rankCounts.Select(rc => rc.Rank).ToList() };
        }
        if (rankCounts[0].Count == 2)
        {
            return new HandEvaluationResult { Rank = PokerHandRank.Pair, HighCardRanks = rankCounts.Select(rc => rc.Rank).ToList() };
        }

        return new HandEvaluationResult { Rank = PokerHandRank.HighCard, HighCardRanks = ranks };
    }

    private bool IsStraight(List<int> ranks)
    {
        // ranks are sorted descending.
        // Check for the special case of 10-J-Q-K-A ("Broadway") where Ace (1) is at the end.
        if (ranks.SequenceEqual(new List<int> { 13, 12, 11, 10, 1 }))
        {
            return true;
        }

        // Check for all other consecutive straights.
        // This includes the A-2-3-4-5 "Wheel", which would be [5, 4, 3, 2, 1].
        for (int i = 0; i < ranks.Count - 1; i++)
        {
            if (ranks[i] - ranks[i+1] != 1)
            {
                return false;
            }
        }
        return true;
    }
}