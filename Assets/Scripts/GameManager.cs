
using System.Collections.Generic;
using UnityEngine;

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
    public List<Card> deck = new List<Card>();

    void Start()
    {
        currentState = GameState.Setup;
        Debug.Log("Game Manager is ready. Current state: " + currentState);
        
        CreateDeck();
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
            }
        }

        Debug.Log("Deck created with " + deck.Count + " cards.");

        ShuffleDeck();
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
