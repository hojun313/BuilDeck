using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode; // NetworkBehaviour를 위해 추가

using System.Collections;

public class Player : NetworkBehaviour
{
    public NetworkList<ulong> handNetworkIds; // 카드의 NetworkObjectId를 저장
    public string playerName;
    public NetworkVariable<bool> hasUsedTrashAndRefill = new NetworkVariable<bool>(false); // 현재 턴에 필드 덱 리필을 사용했는지 여부

    void Awake()
    {
        handNetworkIds = new NetworkList<ulong>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // 클라이언트 측에서 자신의 핸드 디스플레이를 업데이트하기 위해 구독
            handNetworkIds.OnListChanged += OnHandNetworkIdsChanged;
            // 초기 핸드 표시
            StartCoroutine(UpdateHandDisplayNextFrame());
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            handNetworkIds.OnListChanged -= OnHandNetworkIdsChanged;
        }
    }

    private void OnHandNetworkIdsChanged(NetworkListEvent<ulong> changeEvent)
    {
        // 핸드 리스트가 변경될 때마다 UI 업데이트 (다음 프레임에)
        if (IsOwner)
        {
            StartCoroutine(UpdateHandDisplayNextFrame());
        }
    }

    private IEnumerator UpdateHandDisplayNextFrame()
    {
        // 다음 프레임까지 기다립니다.
        yield return null;

        HandDisplay handDisplay = GetComponent<HandDisplay>();
        if (handDisplay != null)
        {
            List<Card> currentHandCards = new List<Card>();
            foreach (ulong cardId in handNetworkIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
                {
                    Card card = networkObject.GetComponent<Card>();
                    if (card != null)
                    {
                        currentHandCards.Add(card);
                    }
                }
            }
            handDisplay.DisplayHand(currentHandCards);
        }
    }

    public void RequestSelectCard(Card card)
    {
        if (!IsOwner) return; // 자기 자신만 카드 선택을 요청할 수 있습니다.

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SelectCardServerRpc(card.NetworkObjectId);
        }
    }

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
        if (!IsServer) return; // 서버에서만 호출되도록
        handNetworkIds.Add(card.NetworkObjectId);
        Debug.Log(playerName + " received " + card.cardSuit + " " + card.cardRank);
    }

    public void ClearHand()
    {
        if (!IsServer) return; // 서버에서만 호출되도록
        handNetworkIds.Clear();
        Debug.Log(playerName + "'s hand cleared.");
    }

    public class HandEvaluationResult
    {
        public PokerHandRank Rank { get; set; }
        public List<int> HighCardRanks { get; set; } = new List<int>();
    }

    public HandEvaluationResult EvaluateHand()
    {
        // NetworkList에서 실제 Card 객체를 가져와서 평가
        List<Card> currentHandCards = new List<Card>();
        foreach (ulong cardId in handNetworkIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
            {
                Card card = networkObject.GetComponent<Card>();
                if (card != null)
                {
                    currentHandCards.Add(card);
                }
            }
        }

        if (currentHandCards.Count != 5)
        {
            Debug.LogWarning("Hand must contain 5 cards for poker evaluation.");
            return new HandEvaluationResult { Rank = PokerHandRank.HighCard };
        }

        // Sort ranks descending, Ace is 1 by default.
        List<int> ranks = currentHandCards.Select(card => (int)card.cardRank.Value).OrderByDescending(r => r).ToList();
        List<Card.Suit> suits = currentHandCards.Select(card => card.cardSuit.Value).ToList();

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