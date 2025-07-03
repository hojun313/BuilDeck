
using UnityEngine;

public class Card : MonoBehaviour
{
    public enum Suit { Spade, Heart, Diamond, Club }
    public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }

    public Suit cardSuit;
    public Rank cardRank;
    public Player owner; // 이 카드를 소유한 플레이어

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnMouseDown()
    {
        if (gameManager != null)
        {
            gameManager.HandleCardSelection(this);
        }
    }
}
