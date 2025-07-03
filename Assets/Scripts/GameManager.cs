
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections; // Coroutine을 위해 추가

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
    public Card selectedCardFromHand = null;
    public Card selectedCardFromField = null;
    public List<Card> deck = new List<Card>();
    public List<Card> fieldDeck = new List<Card>();
    public List<Card> discardPile = new List<Card>(); // 새로 추가된 죽은 카드 더미
    public int fieldDeckSize = 5; // 필드 덱의 카드 수
    public FieldDeckDisplay fieldDeckDisplay; // 필드 덱 디스플레이 참조
    public GameUI gameUI; // GameUI 참조

    private const float cardMoveDuration = 0.3f; // 카드 이동 애니메이션 지속 시간

    void Start()
    {
        currentState = GameState.Setup;
        Debug.Log("Game Manager is ready. Current state: " + currentState);

        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }
        
        CreateDeck();
        ShuffleDeck();
        CreatePlayers();
        DealCardsToPlayers(5); // 각 플레이어에게 5장씩 분배 (테스트용)
        FillFieldDeck(); // 필드 덱 채우기

        currentState = GameState.Playing;
        UpdateTurnInfo();
        if (gameUI != null) gameUI.UpdateButtonStates(currentState, players[currentPlayerIndex]);
    }

    void UpdateTurnInfo()
    {
        if (gameUI != null)
        {
            Player currentPlayer = players[currentPlayerIndex];
            gameUI.UpdateStatusText($"Current Turn: {currentPlayer.playerName}");
            gameUI.UpdateButtonStates(currentState, currentPlayer);
        }
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
                    cardToDeal.owner = player; // 카드의 소유자 설정
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
        Debug.Log($"[FillFieldDeck] Initial Deck Count: {deck.Count}, Discard Pile Count: {discardPile.Count}");
        for (int i = 0; i < fieldDeckSize; i++)
        {
            if (deck.Count == 0) // 덱에 카드가 없으면 discardPile을 셔플하여 덱으로 가져옴
            {
                if (discardPile.Count > 0)
                {
                    Debug.Log("Deck is empty. Shuffling discard pile into deck.");
                    foreach (Card card in discardPile)
                    {
                        deck.Add(card);
                    }
                    discardPile.Clear();
                    ShuffleDeck(); // 덱을 다시 셔플
                    Debug.Log($"[FillFieldDeck] After reshuffle: Deck Count: {deck.Count}, Discard Pile Count: {discardPile.Count}");
                }
                else
                {
                    Debug.LogWarning("No cards in deck or discard pile to fill the field deck.");
                    break; // 덱과 discardPile 모두 비어있으면 종료
                }
            }

            if (deck.Count > 0) // Check again after potential reshuffle
            {
                Card cardToField = deck[0];
                deck.RemoveAt(0);
                fieldDeck.Add(cardToField);
            }
            else
            {
                Debug.LogWarning("Not enough cards in the deck to fill the field deck after reshuffle.");
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
        UpdateTurnInfo(); // 턴 정보를 UI에 업데이트
    }

    public void HandleCardSelection(Card card)
    {
        Player currentPlayer = players[currentPlayerIndex];

        // 게임이 플레이 중 상태가 아니면 카드 선택을 막습니다.
        if (currentState != GameState.Playing)
        {
            Debug.Log("Cannot select cards when game is not in Playing state.");
            return;
        }

        // 이 카드가 현재 플레이어의 핸드에 있는지 확인
        if (currentPlayer.hand.Contains(card))
        {
            // 현재 턴 플레이어의 카드인지 확인
            if (card.owner != currentPlayer)
            {
                Debug.LogWarning("Cannot select another player's card.");
                return;
            }

            if (selectedCardFromHand == card) // 이미 선택된 핸드 카드를 다시 클릭한 경우
            {
                selectedCardFromHand.GetComponent<CardDisplay>().SetSelected(false);
                selectedCardFromHand = null;
                Debug.Log("Deselected from hand: " + card.name);
            }
            else
            {
                if (selectedCardFromHand != null)
                {
                    selectedCardFromHand.GetComponent<CardDisplay>().SetSelected(false);
                }
                selectedCardFromHand = card;
                selectedCardFromHand.GetComponent<CardDisplay>().SetSelected(true);
                Debug.Log("Selected from hand: " + card.name);
            }
        }
        // 이 카드가 필드 덱에 있는지 확인 (필드 덱 카드는 항상 선택 가능)
        else if (fieldDeck.Contains(card))
        {
            if (selectedCardFromField == card) // 이미 선택된 필드 카드를 다시 클릭한 경우
            {
                selectedCardFromField.GetComponent<CardDisplay>().SetSelected(false);
                selectedCardFromField = null;
                Debug.Log("Deselected from field: " + card.name);
            }
            else
            {
                if (selectedCardFromField != null)
                {
                    selectedCardFromField.GetComponent<CardDisplay>().SetSelected(false);
                }
                selectedCardFromField = card;
                selectedCardFromField.GetComponent<CardDisplay>().SetSelected(true);
                Debug.Log("Selected from field: " + card.name);
            }
        }

        // 두 카드가 모두 선택되었으면 교환을 시도합니다.
        if (selectedCardFromHand != null && selectedCardFromField != null)
        {
            SwapSelectedCards();
        }
    }

    void SwapSelectedCards()
    {
        Player currentPlayer = players[currentPlayerIndex];

        int playerCardIndex = currentPlayer.hand.IndexOf(selectedCardFromHand);
        int fieldCardIndex = fieldDeck.IndexOf(selectedCardFromField);

        if (playerCardIndex == -1 || fieldCardIndex == -1)
        {
            Debug.LogError("Selected cards are not valid for swapping.");
            return;
        }

        // 선택 효과 해제
        selectedCardFromHand.GetComponent<CardDisplay>().SetSelected(false);
        selectedCardFromField.GetComponent<CardDisplay>().SetSelected(false);

        // 카드 교환 (데이터)
        Card tempPlayerCard = selectedCardFromHand;
        Card tempFieldCard = selectedCardFromField;

        currentPlayer.hand[playerCardIndex] = tempFieldCard;
        fieldDeck[fieldCardIndex] = tempPlayerCard;

        // 애니메이션 시작
        StartCoroutine(AnimateCardSwap(tempPlayerCard, tempFieldCard, currentPlayer.transform, fieldDeckDisplay.transform, playerCardIndex, fieldCardIndex));

        Debug.Log($"{currentPlayer.playerName} swapped {tempPlayerCard.name} with {tempFieldCard.name}");

        // 선택 변수 초기화
        selectedCardFromHand = null;
        selectedCardFromField = null;

        // 턴 넘기기는 애니메이션 완료 후 진행
    }

    private IEnumerator AnimateCardSwap(Card playerCard, Card fieldCard, Transform playerHandParent, Transform fieldDeckParent, int playerCardOriginalIndex, int fieldCardOriginalIndex)
    {
        // 애니메이션을 위해 잠시 부모를 GameManager로 변경
        playerCard.transform.SetParent(this.transform);
        fieldCard.transform.SetParent(this.transform);

        Vector3 playerCardStartPos = playerCard.transform.position;
        Vector3 fieldCardStartPos = fieldCard.transform.position;

        // 목표 위치는 애니메이션 완료 후 HandDisplay와 FieldDeckDisplay가 다시 설정할 것이므로, 여기서는 임시로 서로의 시작 위치를 목표로 설정합니다.
        // 실제로는 HandDisplay와 FieldDeckDisplay의 DisplayHand/DisplayFieldDeck이 호출된 후 최종 위치가 결정됩니다.
        Vector3 playerCardTargetPos = fieldCardStartPos; 
        Vector3 fieldCardTargetPos = playerCardStartPos;

        float timer = 0f;
        while (timer < cardMoveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / cardMoveDuration;

            playerCard.transform.position = Vector3.Lerp(playerCardStartPos, playerCardTargetPos, t);
            fieldCard.transform.position = Vector3.Lerp(fieldCardStartPos, fieldCardTargetPos, t);
            yield return null;
        }

        // 애니메이션 완료 후 최종 위치 설정 (정확한 위치는 DisplayHand/DisplayFieldDeck이 담당)
        playerCard.transform.position = playerCardTargetPos;
        fieldCard.transform.position = fieldCardTargetPos;

        // 카드의 소유자 업데이트 (데이터는 이미 SwapSelectedCards에서 업데이트됨)
        playerCard.owner = players[currentPlayerIndex]; // 필드에서 핸드로 온 카드
        fieldCard.owner = null; // 핸드에서 필드로 간 카드

        // 디스플레이 업데이트 (부모 재설정 및 재정렬)
        players[currentPlayerIndex].GetComponent<HandDisplay>().DisplayHand(players[currentPlayerIndex].hand);
        fieldDeckDisplay.DisplayFieldDeck(fieldDeck);

        AdvanceTurn(); // 턴 넘기기
    }

    public void SwapCardWithFieldDeck(int playerCardIndex, int fieldCardIndex)
    {
        // 이 메소드는 이제 직접 사용되지 않고, 카드 클릭을 통해 호출되는 SwapSelectedCards로 대체됩니다.
        // 하지만 여전히 다른 로직에서 필요할 수 있으므로 남겨두거나, 필요 없다면 삭제할 수 있습니다.
        // 지금은 남겨두겠습니다.
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
        Debug.Log("Trashing current field deck and refilling.");
        // 현재 필드 덱의 카드들을 discardPile로 이동
        foreach (Card card in fieldDeck)
        {
            discardPile.Add(card); // discardPile에 추가
        }
        fieldDeck.Clear(); // 필드 덱 비우기

        FillFieldDeck(); // 새로운 카드로 필드 덱 채우기
        Debug.Log("Field deck refilled. Now player can choose to swap a card.");
    }

    public void DeclareStop()
    {
        currentState = GameState.GameOver;
        Debug.Log(players[currentPlayerIndex].playerName + " declared stop! Game is over. Proceeding to result checking.");
        if (gameUI != null) gameUI.UpdateButtonStates(currentState, null); // currentPlayer를 null로 전달
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
            if (gameUI != null)
            {
                gameUI.UpdateStatusText($"Winner: {winner.playerName} with a {bestHand.Rank}!");
            }
        }
        else
        {
            Debug.Log("It's a draw! No single winner determined.");
            if (gameUI != null)
            {
                gameUI.UpdateStatusText("It's a draw!");
            }
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
