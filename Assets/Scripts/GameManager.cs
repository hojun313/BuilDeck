using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections; // Coroutine을 위해 추가
using Unity.Netcode; // NetworkBehaviour를 위해 추가

public class GameManager : NetworkBehaviour
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
    
    public NetworkVariable<int> deckCount = new NetworkVariable<int>();
    public NetworkVariable<int> discardPileCount = new NetworkVariable<int>();

    public List<Card> discardPile = new List<Card>(); // 새로 추가된 죽은 카드 더미
    public int fieldDeckSize = 5; // 필드 덱의 카드 수
    public FieldDeckDisplay fieldDeckDisplay; // 필드 덱 디스플레이 참조
    public GameUI gameUI; // GameUI 참조

    private const float cardMoveDuration = 0.3f; // 카드 이동 애니메이션 지속 시간

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Game Manager is ready. Current state: " + currentState);

            if (gameUI == null)
            {
                gameUI = FindObjectOfType<GameUI>();
            }
            
            CreateDeck();
            ShuffleDeck();
            // CreatePlayers(); // NetworkManager가 플레이어를 생성하므로 주석 처리
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected; // 클라이언트 연결 이벤트 구독

            // 모든 클라이언트가 연결된 후 게임 시작 (테스트를 위해 임시로 바로 시작)
            // StartGame();
        }
    }

    void HandleClientConnected(ulong clientId)
    {
        // 클라이언트가 연결될 때마다 해당 클라이언트의 플레이어 오브젝트를 찾아서 players 리스트에 추가
        // NetworkManager가 플레이어 오브젝트를 생성한 후 호출되므로, 여기서 플레이어 오브젝트를 찾을 수 있습니다.
        NetworkObject clientNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (clientNetworkObject != null)
        {
            Player player = clientNetworkObject.GetComponent<Player>();
            if (player != null && !players.Contains(player))
            {
                player.playerName = "Player " + (players.Count + 1);
                players.Add(player);
                Debug.Log(player.playerName + " connected.");

                // OpponentHandDisplay 설정은 이제 SetupOpponentDisplaysClientRpc에서 처리됩니다.
            }
        }

        // 모든 플레이어가 연결되면 게임 시작 (예시: numberOfPlayers에 도달하면)
        if (players.Count == numberOfPlayers)
        {
            StartGame();
        }

        // 모든 클라이언트가 연결된 후, 각 클라이언트에게 상대방 핸드 디스플레이를 설정하도록 RPC 호출
        if (IsServer && players.Count == numberOfPlayers)
        {
            SetupOpponentDisplaysClientRpc();
        }
    }

    [ClientRpc]
    void SetupOpponentDisplaysClientRpc()
    {
        // 로컬 클라이언트의 Player 객체를 찾습니다.
        Player localPlayer = null;
        foreach (var p in players)
        {
            if (p.IsOwner)
            {
                localPlayer = p;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogError("Local player not found for setting up opponent displays.");
            return;
        }

        // 씬에 있는 OpponentHandDisplay 인스턴스를 찾습니다.
        OpponentHandDisplay opponentHandDisplay = FindObjectOfType<OpponentHandDisplay>();
        if (opponentHandDisplay == null)
        {
            Debug.LogError("OpponentHandDisplay not found in the scene.");
            return;
        }

        // 상대방 플레이어를 찾아 OpponentHandDisplay에 연결합니다.
        foreach (var p in players)
        {
            if (p != localPlayer) // 자신이 아닌 다른 플레이어
            {
                opponentHandDisplay.SetTargetPlayer(p);
                break; // 현재는 2인 플레이를 가정하므로 첫 번째 상대방만 설정
            }
        }
    }

    void StartGame()
    {
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
        // 서버에서만 카드 생성 및 스폰
        if (!IsServer) return;

        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                GameObject newCardObject = Instantiate(cardPrefab, new Vector3(0, -10, 0), Quaternion.identity); // 초기에 보이지 않는 위치로 설정
                Card newCard = newCardObject.GetComponent<Card>();
                newCard.cardSuit.Value = suit;
                newCard.cardRank.Value = rank;
                newCardObject.name = suit.ToString() + " " + rank.ToString();
                
                // NetworkObject 컴포넌트를 가져와 스폰합니다.
                NetworkObject networkObject = newCardObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn(); // 모든 클라이언트에 카드를 스폰합니다.
                    Debug.Log($"[GameManager] Spawning card: {newCardObject.name}, NetworkObjectId: {networkObject.NetworkObjectId}");
                }
                else
                {
                    Debug.LogError("CardPrefab is missing NetworkObject component!");
                    Destroy(newCardObject); // NetworkObject가 없으면 오브젝트를 파괴합니다.
                    continue;
                }

                deck.Add(newCard); // 서버의 로컬 덱 리스트에 추가
            }
        }
        deckCount.Value = deck.Count; // 덱 카운트 업데이트
        Debug.Log("Deck created and spawned with " + deck.Count + " cards.");
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
        // 이 메소드는 더 이상 직접 플레이어를 생성하지 않습니다.
        // NetworkManager가 플레이어 프리팹을 스폰하고, HandleClientConnected에서 players 리스트에 추가합니다.
    }

    void DealCardsToPlayers(int cardsPerPlayer)
    {
        // 서버에서만 카드 분배 로직 실행
        if (!IsServer) return;

        // 모든 플레이어에게 카드 분배
        foreach (Player player in players)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                if (deck.Count > 0)
                {
                    Card cardToDeal = deck[0];
                    deck.RemoveAt(0);
                    player.AddCardToHand(cardToDeal);
                    cardToDeal.ownerClientId.Value = player.OwnerClientId; // 카드의 소유자 ClientId 설정
                    cardToDeal.NetworkObject.TrySetParent(player.NetworkObject);
                }
                else
                {
                    Debug.LogWarning("Not enough cards in the deck to deal to all players.");
                    return;
                }
            }
        }
        deckCount.Value = deck.Count; // 덱 카운트 업데이트
    }

    void FillFieldDeck()
    {
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
                    discardPileCount.Value = discardPile.Count; // 죽은 카드 더미 카운트 업데이트
                }
                else
                {
                    Debug.LogWarning("Not enough cards in the deck or discard pile to fill the field deck.");
                    break; // 덱과 discardPile 모두 비어있으면 종료
                }
            }

            if (deck.Count > 0)
            {
                Card cardToField = deck[0];
                deck.RemoveAt(0);
                // fieldDeck.Add(cardToField); // 기존 fieldDeck 대신 NetworkList에 NetworkObjectId 추가
                if (fieldDeckDisplay != null)
                {
                    fieldDeckDisplay.fieldDeckCardIds.Add(cardToField.NetworkObjectId);
                }
            }
            else
            {
                Debug.LogWarning("Not enough cards in the deck to fill the field deck.");
                break;
            }
        }
        deckCount.Value = deck.Count; // 덱 카운트 업데이트
        Debug.Log("Field deck filled with " + fieldDeckDisplay.fieldDeckCardIds.Count + " cards.");
        if (fieldDeckDisplay != null)
        {
            fieldDeckDisplay.DisplayFieldDeck(); // 매개변수 없이 호출
        }
        Debug.Log("Current turn: " + players[currentPlayerIndex].playerName);
    }

    public void AdvanceTurn()
    {
        // 현재 턴 플레이어의 hasUsedTrashAndRefill 상태를 초기화합니다.
        players[currentPlayerIndex].hasUsedTrashAndRefill.Value = false;

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log("Current turn: " + players[currentPlayerIndex].playerName);
        UpdateTurnInfo(); // 턴 정보를 UI에 업데이트
    }

    public void HandleCardSelection(Card card)
    {
        // 이 메서드는 이제 서버에서만 실행됩니다.
        if (!IsServer) return;

        Player currentPlayer = players[currentPlayerIndex];

        Debug.Log($"[GameManager] HandleCardSelection for {card.name}. Current Hand Selection: {(selectedCardFromHand != null ? selectedCardFromHand.name : "None")}, Field Selection: {(selectedCardFromField != null ? selectedCardFromField.name : "None")}");

        // 게임이 플레이 중 상태가 아니면 카드 선택을 막습니다.
        if (currentState != GameState.Playing)
        {
            Debug.Log("Cannot select cards when game is not in Playing state.");
            return;
        }

        // 이 카드가 현재 플레이어의 핸드에 있는지 확인
        if (currentPlayer.handNetworkIds.Contains(card.NetworkObjectId))
        {
            if (selectedCardFromHand == card) // 이미 선택된 핸드 카드를 다시 클릭한 경우
            {
                Debug.Log($"[GameManager] Deselecting already selected hand card: {card.name}");
                selectedCardFromHand.isSelected.Value = false;
                selectedCardFromHand = null;
            }
            else
            {
                if (selectedCardFromHand != null)
                {
                    Debug.Log($"[GameManager] Deselecting previous hand card: {selectedCardFromHand.name}");
                    selectedCardFromHand.isSelected.Value = false;
                }
                selectedCardFromHand = card;
                selectedCardFromHand.isSelected.Value = true;
                Debug.Log($"[GameManager] Selected from hand: {card.name}");
            }
        }
        // 이 카드가 필드 덱에 있는지 확인 (필드 덱 카드는 항상 선택 가능)
        else if (fieldDeckDisplay.fieldDeckCardIds.Contains(card.NetworkObjectId))
        {
            if (selectedCardFromField == card) // 이미 선택된 필드 카드를 다시 클릭한 경우
            {
                Debug.Log($"[GameManager] Deselecting already selected field card: {card.name}");
                selectedCardFromField.isSelected.Value = false;
                selectedCardFromField = null;
            }
            else
            {
                if (selectedCardFromField != null)
                {
                    Debug.Log($"[GameManager] Deselecting previous field card: {selectedCardFromField.name}");
                    selectedCardFromField.isSelected.Value = false;
                }
                selectedCardFromField = card;
                selectedCardFromField.isSelected.Value = true;
                Debug.Log($"[GameManager] Selected from field: {card.name}");
            }
        }

        // 두 카드가 모두 선택되었으면 교환을 시도합니다.
        if (selectedCardFromHand != null && selectedCardFromField != null)
        {
            Debug.Log("[GameManager] Both hand and field cards selected. Attempting swap.");
            SwapSelectedCards();
        }
    }

    [ServerRpc(RequireOwnership = false)] // 모든 클라이언트가 호출할 수 있도록 설정
    public void SelectCardServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        Player requestingPlayer = null;
        foreach (var p in players)
        {
            if (p.OwnerClientId == clientId)
            {
                requestingPlayer = p;
                break;
            }
        }

        if (requestingPlayer == null)
        {
            Debug.LogError($"Requesting player not found for clientId: {clientId}");
            return;
        }

        // 현재 턴의 플레이어인지 확인
        if (players[currentPlayerIndex] != requestingPlayer)
        {
            Debug.LogWarning($"{requestingPlayer.playerName} tried to select a card, but it's not their turn.");
            return;
        }

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
        {
            Card card = networkObject.GetComponent<Card>();
            if (card != null)
            {
                HandleCardSelection(card);
            }
        }
    }

    void SwapSelectedCards()
    {
        Player currentPlayer = players[currentPlayerIndex];

        int playerCardIndex = -1;
        for(int i = 0; i < currentPlayer.handNetworkIds.Count; i++)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentPlayer.handNetworkIds[i], out NetworkObject networkObject) && networkObject.GetComponent<Card>() == selectedCardFromHand)
            {
                playerCardIndex = i;
                break;
            }
        }
        int fieldCardIndex = -1;
        for(int i = 0; i < fieldDeckDisplay.fieldDeckCardIds.Count; i++)
        {
            if(NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(fieldDeckDisplay.fieldDeckCardIds[i], out NetworkObject networkObject) && networkObject.GetComponent<Card>() == selectedCardFromField)
            {
                fieldCardIndex = i;
                break;
            }
        }

        if (playerCardIndex == -1 || fieldCardIndex == -1)
        {
            Debug.LogError("Selected cards are not valid for swapping.");
            return;
        }

        // 선택 효과 해제
        selectedCardFromHand.isSelected.Value = false;
        selectedCardFromField.isSelected.Value = false;

        // 카드 교환 (데이터)
        Card tempPlayerCard = selectedCardFromHand;
        Card tempFieldCard = selectedCardFromField;

        // NetworkList의 요소를 변경할 때는 인덱서를 사용합니다.
        currentPlayer.handNetworkIds[playerCardIndex] = tempFieldCard.NetworkObjectId;
        fieldDeckDisplay.fieldDeckCardIds[fieldCardIndex] = tempPlayerCard.NetworkObjectId;

        Card playerCard = selectedCardFromHand;
        Card fieldCard = selectedCardFromField;

        // 데이터 동기화 후 즉시 디스플레이 업데이트
        playerCard.ownerClientId.Value = 0; // 핸드에서 필드로 간 카드 (소유자 없음)
        fieldCard.ownerClientId.Value = currentPlayer.OwnerClientId; // 필드에서 핸드로 온 카드

        Debug.Log($"{currentPlayer.playerName} swapped {playerCard.name} with {fieldCard.name}");

        // 선택 변수 초기화
        selectedCardFromHand = null;
        selectedCardFromField = null;

        AdvanceTurn(); // 턴 넘기기
    }

    private IEnumerator AnimateCardSwap(Card playerCard, Card fieldCard, Transform playerHandParent, Transform fieldDeckParent, int playerCardOriginalIndex, int fieldCardOriginalIndex)
    {
        // // 애니메이션을 위해 잠시 부모를 GameManager로 변경
        // playerCard.transform.SetParent(this.transform);
        // fieldCard.transform.SetParent(this.transform);

        // Vector3 playerCardStartPos = playerCard.transform.position;
        // Vector3 fieldCardStartPos = fieldCard.transform.position;

        // // 목표 위치는 애니메이션 완료 후 HandDisplay와 FieldDeckDisplay가 다시 설정할 것이므로, 여기서는 임시로 서로의 시작 위치를 목표로 설정합니다.
        // // 실제로는 HandDisplay와 FieldDeckDisplay의 DisplayHand/DisplayFieldDeck이 호출된 후 최종 위치가 결정됩니다.
        // Vector3 playerCardTargetPos = fieldCardStartPos; 
        // Vector3 fieldCardTargetPos = playerCardStartPos;

        // float timer = 0f;
        // while (timer < cardMoveDuration)
        // {
        //     timer += Time.deltaTime;
        //     float t = timer / cardMoveDuration;

        //     playerCard.transform.position = Vector3.Lerp(playerCardStartPos, playerCardTargetPos, t);
        //     fieldCard.transform.position = Vector3.Lerp(fieldCardStartPos, fieldCardTargetPos, t);
        //     yield return null;
        // }

        // // 애니메이션 완료 후 최종 위치 설정 (정확한 위치는 DisplayHand/DisplayFieldDeck이 담당)
        // playerCard.transform.position = playerCardTargetPos;
        // fieldCard.transform.position = fieldCardTargetPos;

        // 카드의 소유자 업데이트 (데이터는 이미 SwapSelectedCards에서 업데이트됨)
        // playerCard.ownerClientId.Value = 0; // 핸드에서 필드로 간 카드 (소유자 없음)
        // fieldCard.ownerClientId.Value = players[currentPlayerIndex].OwnerClientId; // 필드에서 핸드로 온 카드

        // 디스플레이 업데이트 (부모 재설정 및 재정렬)
        // UpdateHandDisplayClientRpc(players[currentPlayerIndex].OwnerClientId);
        // UpdateFieldDeckClientRpc();

        AdvanceTurn(); // 턴 넘기기
        yield return null; // 코루틴이므로 yield return null이 필요합니다.
    }

    public void SwapCardWithFieldDeck(int playerCardIndex, int fieldCardIndex)
    {
        // 이 메소드는 이제 직접 사용되지 않고, 카드 클릭을 통해 호출되는 SwapSelectedCards로 대체됩니다.
        // 하지만 여전히 다른 로직에서 필요할 수 있으므로 남겨두거나, 필요 없다면 삭제할 수 있습니다.
        // 지금은 남겨두겠습니다.
        Player currentPlayer = players[currentPlayerIndex];

        // NetworkList의 인덱스를 직접 사용하는 대신, NetworkObjectId를 통해 카드를 찾습니다.
        Card playerCard = null;
        if (playerCardIndex >= 0 && playerCardIndex < currentPlayer.handNetworkIds.Count)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentPlayer.handNetworkIds[playerCardIndex], out NetworkObject networkObject))
            {
                playerCard = networkObject.GetComponent<Card>();
            }
        }

        if (playerCard == null)
        {
            Debug.LogError("Invalid player card index or card not found.");
            return;
        }

        if (fieldCardIndex < 0 || fieldCardIndex >= fieldDeckDisplay.fieldDeckCardIds.Count)
        {
            Debug.LogError("Invalid field card index.");
            return;
        }

        Card fieldCard = NetworkManager.Singleton.SpawnManager.SpawnedObjects[fieldDeckDisplay.fieldDeckCardIds[fieldCardIndex]].GetComponent<Card>();

        // 데이터 교환
        currentPlayer.handNetworkIds[playerCardIndex] = fieldCard.NetworkObjectId;
        fieldDeckDisplay.fieldDeckCardIds[fieldCardIndex] = playerCard.NetworkObjectId;

        Debug.Log(currentPlayer.playerName + " swapped " + playerCard.name + " with " + fieldCard.name + " from field deck.");

        // 핸드 디스플레이 업데이트
        HandDisplay handDisplay = currentPlayer.GetComponent<HandDisplay>();
        if (handDisplay != null)
        {
            // HandDisplay는 NetworkList를 직접 처리하도록 이미 수정되었으므로, player.hand 대신 player.handNetworkIds를 전달합니다.
            // 하지만 HandDisplay.DisplayHand는 List<Card>를 받으므로, NetworkList에서 Card 객체를 추출해야 합니다.
            handDisplay.DisplayHand(GetPlayerHandCards(currentPlayer));
        }

        // 필드 덱 디스플레이 업데이트
        if (fieldDeckDisplay != null)
        {
            fieldDeckDisplay.DisplayFieldDeck();
        }

        AdvanceTurn();
    }

    public void TrashAndRefillFieldDeck()
    {
        Debug.Log("Trashing current field deck and refilling.");
        // 현재 필드 덱의 카드들을 discardPile로 이동
        foreach (ulong cardId in fieldDeckDisplay.fieldDeckCardIds)
        {
            discardPile.Add(NetworkManager.Singleton.SpawnManager.SpawnedObjects[cardId].GetComponent<Card>()); // discardPile에 추가
        }
        discardPileCount.Value = discardPile.Count; // 죽은 카드 더미 카운트 업데이트
        fieldDeckDisplay.fieldDeckCardIds.Clear(); // 필드 덱 비우기

        FillFieldDeck(); // 새로운 카드로 필드 덱 채우기
        Debug.Log("Field deck refilled. Now player can choose to swap a card.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestTrashAndRefillServerRpc(ServerRpcParams rpcParams = default)
    {
        Player requestingPlayer = players.Find(p => p.OwnerClientId == rpcParams.Receive.SenderClientId);
        if (requestingPlayer == null)
        {
            Debug.LogError($"Requesting player not found for clientId: {rpcParams.Receive.SenderClientId}");
            return;
        }

        if (players[currentPlayerIndex] != requestingPlayer)
        {
            Debug.LogWarning($"{requestingPlayer.playerName} tried to trash and refill the field deck, but it's not their turn.");
            return;
        }

        if (requestingPlayer.hasUsedTrashAndRefill.Value)
        {
            Debug.LogWarning($"{requestingPlayer.playerName} already used Trash and Refill this turn.");
            return;
        }

        TrashAndRefillFieldDeck();
        requestingPlayer.hasUsedTrashAndRefill.Value = true; // 사용했음을 표시
    }

    public void DeclareStop()
    {
        currentState = GameState.GameOver;
        Debug.Log(players[currentPlayerIndex].playerName + " declared stop! Game is over. Proceeding to result checking.");
        if (gameUI != null) gameUI.UpdateButtonStates(currentState, null); // currentPlayer를 null로 전달
        DetermineWinner();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestDeclareStopServerRpc(ServerRpcParams rpcParams = default)
    {
        if (players[currentPlayerIndex].OwnerClientId != rpcParams.Receive.SenderClientId)
        {
            Debug.LogWarning("A non-turn player tried to declare stop.");
            return;
        }
        DeclareStop();
    }

    void DetermineWinner()
    {
        Player winner = null;
        Player.HandEvaluationResult bestHand = null;

        Debug.Log("\n--- Determining Winner ---");
        foreach (Player player in players)
        {
            // NetworkList<ulong>에서 실제 Card 객체 리스트를 생성
            List<Card> playerActualHand = new List<Card>();
            foreach (ulong cardId in player.handNetworkIds)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
                {
                    Card card = networkObject.GetComponent<Card>();
                    if (card != null)
                    {
                        playerActualHand.Add(card);
                    }
                }
            }

            Player.HandEvaluationResult currentHand = player.EvaluateHand();
            Debug.Log($"{player.playerName}'s hand: {string.Join(", ", playerActualHand.Select(c => c.cardSuit + " " + c.cardRank))} (Rank: {currentHand.Rank})");

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

    // 헬퍼 메소드: Player의 handNetworkIds에서 실제 Card 객체 리스트를 가져옵니다.
    private List<Card> GetPlayerHandCards(Player player)
    {
        List<Card> handCards = new List<Card>();
        foreach (ulong cardId in player.handNetworkIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardId, out NetworkObject networkObject))
            {
                Card card = networkObject.GetComponent<Card>();
                if (card != null)
                {
                    handCards.Add(card);
                }
            }
        }
        return handCards;
    }

    [ClientRpc]
    void UpdateAllDisplaysClientRpc()
    {
        foreach (Player player in players)
        {
            // 클라이언트 자신에게 속한 핸드만 업데이트하도록 확인
            if (player.IsOwner)
            {
                player.GetComponent<HandDisplay>().DisplayHand(GetPlayerHandCards(player));
            }
        }
        fieldDeckDisplay.DisplayFieldDeck();
    }
}