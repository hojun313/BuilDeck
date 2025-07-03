
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Setup,
        Playing,
        GameOver
    }

    public GameState currentState;

    public GameObject cardPrefab; // 여기에 CardPrototype 프리팹을 연결합니다.
    public GameObject playerPrefab; // 플레이어 오브젝트 프리팹 (나중에 생성)
    public int numberOfPlayers = 2; // 테스트를 위한 플레이어 수
    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0; // 현재 턴 플레이어 인덱스
    public List<Card> deck = new List<Card>();
    public List<Card> fieldDeck = new List<Card>();
    public int fieldDeckSize = 5; // 필드 덱의 카드 수
    public FieldDeckDisplay fieldDeckDisplay; // 필드 덱 디스플레이 참조

    void Start()
    {
        currentState = GameState.Setup;
        Debug.Log("Game Manager is ready. Current state: " + currentState);
        
        CreateDeck();
        ShuffleDeck();
        CreatePlayers();
        DealCardsToPlayers(5); // 각 플레이어에게 5장씩 분배 (테스트용)
        FillFieldDeck(); // 필드 덱 채우기
    }

    void CreateDeck()
    {
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                GameObject newCardObject = Instantiate(cardPrefab, transform.position, Quaternion.identity);
                Card newCard = newCardObject.GetComponent<Card>();
                newCard.cardSuit = suit;
                newCard.cardRank = rank;
                newCardObject.name = suit.ToString() + " " + rank.ToString();
                deck.Add(newCard);

                // CardDisplay 컴포넌트 설정
                CardDisplay cardDisplay = newCardObject.GetComponent<CardDisplay>();
                if (cardDisplay != null)
                {
                    cardDisplay.SetCard(newCard);
                }
            }
        }

        Debug.Log("Deck created with " + deck.Count + " cards.");
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        Debug.Log("Deck shuffled.");
    }

    void CreatePlayers()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            GameObject playerObject = Instantiate(playerPrefab, this.transform); // GameManager의 자식으로 생성
            Player player = playerObject.GetComponent<Player>();
            player.playerName = "Player " + (i + 1);
            players.Add(player);
            Debug.Log(player.playerName + " created as child of GameManager.");
        }
    }

    void DealCardsToPlayers(int cardsPerPlayer)
    {
        for (int i = 0; i < cardsPerPlayer; i++)
        {
            foreach (Player player in players)
            {
                if (deck.Count > 0)
                {
                    Card cardToDeal = deck[0];
                    deck.RemoveAt(0);
                    player.AddCardToHand(cardToDeal);
                    // 카드 오브젝트의 부모를 플레이어 오브젝트로 설정
                    cardToDeal.transform.SetParent(player.transform);
                    Debug.Log($"Card {cardToDeal.name} parented to {player.name}");
                }
                else
                {
                    Debug.LogWarning("Not enough cards in the deck to deal to all players.");
                    return;
                }
            }
        }
        // 모든 플레이어의 핸드 디스플레이 업데이트
        foreach (Player player in players)
        {
            HandDisplay handDisplay = player.GetComponent<HandDisplay>();
            if (handDisplay != null)
            {
                handDisplay.DisplayHand(player.hand);
            }
        }
    }

    void FillFieldDeck()
    {
        for (int i = 0; i < fieldDeckSize; i++)
        {
            if (deck.Count > 0)
            {
                Card cardToField = deck[0];
                deck.RemoveAt(0);
                fieldDeck.Add(cardToField);
            }
            else
            {
                Debug.LogWarning("Not enough cards in the deck to fill the field deck.");
                break;
            }
        }
        Debug.Log("Field deck filled with " + fieldDeck.Count + " cards.");
        if (fieldDeckDisplay != null)
        {
            fieldDeckDisplay.DisplayFieldDeck(fieldDeck);
        }
        Debug.Log("Current turn: " + players[currentPlayerIndex].playerName);
    }

    public void AdvanceTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log("Current turn: " + players[currentPlayerIndex].playerName);
    }

    public void SwapCardWithFieldDeck(int playerCardIndex, int fieldCardIndex)
    {
        Player currentPlayer = players[currentPlayerIndex];

        if (playerCardIndex < 0 || playerCardIndex >= currentPlayer.hand.Count)
        {
            Debug.LogError("Invalid player card index.");
            return;
        }

        if (fieldCardIndex < 0 || fieldCardIndex >= fieldDeck.Count)
        {
            Debug.LogError("Invalid field card index.");
            return;
        }

        Card playerCard = currentPlayer.hand[playerCardIndex];
        Card fieldCard = fieldDeck[fieldCardIndex];

        currentPlayer.hand[playerCardIndex] = fieldCard;
        fieldDeck[fieldCardIndex] = playerCard;

        Debug.Log(currentPlayer.playerName + " swapped " + playerCard.name + " with " + fieldCard.name + " from field deck.");

        // 핸드 디스플레이 업데이트
        HandDisplay handDisplay = currentPlayer.GetComponent<HandDisplay>();
        if (handDisplay != null)
        {
            handDisplay.DisplayHand(currentPlayer.hand);
        }

        // 필드 덱 디스플레이 업데이트
        if (fieldDeckDisplay != null)
        {
            fieldDeckDisplay.DisplayFieldDeck(fieldDeck);
        }

        AdvanceTurn();
    }

    public void TrashAndRefillFieldDeck()
    {
        Debug.Log("Trashing current field deck.");
        // 현재 필드 덱의 카드 오브젝트들을 비활성화
        foreach (Card card in fieldDeck)
        {
            card.gameObject.SetActive(false);
        }
        fieldDeck.Clear();

        FillFieldDeck(); // 새로운 카드로 필드 덱 채우기
        Debug.Log("Field deck refilled. Now player can choose to swap a card.");
        // 이 시점에서 플레이어는 핸드 카드와 필드 덱 카드 중 하나를 교환하는 행동을 해야 합니다.
        // 실제 게임에서는 UI를 통해 플레이어의 선택을 기다리게 됩니다.
        // 현재는 테스트를 위해 바로 턴을 넘기지 않고, 교환 행동이 완료된 후 턴을 넘기도록 합니다.
    }

    public void DeclareStop()
    {
        currentState = GameState.GameOver;
        Debug.Log(players[currentPlayerIndex].playerName + " declared stop! Game is over. Proceeding to result checking.");
        DetermineWinner();
    }

    void DetermineWinner()
    {
        Player winner = null;
        Player.HandEvaluationResult bestHand = null;

        Debug.Log("\n--- Determining Winner ---");
        foreach (Player player in players)
        {
            Player.HandEvaluationResult currentHand = player.EvaluateHand();
            Debug.Log($"{player.playerName}'s hand: {string.Join(", ", player.hand.Select(c => c.cardSuit + " " + c.cardRank))} (Rank: {currentHand.Rank})");

            if (winner == null || currentHand.Rank > bestHand.Rank)
            {
                winner = player;
                bestHand = currentHand;
            }
            else if (currentHand.Rank == bestHand.Rank)
            {
                // 동점 처리 로직
                for (int i = 0; i < currentHand.HighCardRanks.Count; i++)
                {
                    if (currentHand.HighCardRanks[i] > bestHand.HighCardRanks[i])
                    {
                        winner = player;
                        bestHand = currentHand;
                        break; // 더 높은 카드를 찾았으므로 루프 종료
                    }
                    else if (currentHand.HighCardRanks[i] < bestHand.HighCardRanks[i])
                    {
                        break; // 현재 플레이어의 패가 더 낮으므로 루프 종료
                    }
                    // 두 카드가 같으면 다음 카드로 계속 비교
                }
            }
        }

        if (winner != null)
        {
            Debug.Log($"\nWinner: {winner.playerName} with a {bestHand.Rank}!");
        }
        else
        {
            Debug.Log("It's a draw! No single winner determined.");
        }
        Debug.Log("--------------------------");
    }

    void Update()
    {
        // 게임 상태에 따라 다른 로직을 처리할 수 있습니다.
        switch (currentState)
        {
            case GameState.Setup:
                // 게임 준비 로직
                break;
            case GameState.Playing:
                // 게임 플레이 중 로직
                break;
            case GameState.GameOver:
                // 게임 종료 로직
                break;
        }
    }
}
