
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.Netcode; // Added for NetworkVariable access if needed, though Player has it

public class GameUI : MonoBehaviour
{
    public GameManager gameManager;

    public Button trashAndRefillButton;
    public Button declareStopButton;
    public Button restartButton; // 다시 시작 버튼
    public TextMeshProUGUI statusText;

    public GameObject jokboPanel; // 족보 패널
    public Button jokboButton; // 족보 보기 버튼

    public Sprite trashAndRefillNormalSprite; // New: Assign normal sprite in Inspector
    public Sprite trashAndRefillUsedSprite;   // New: Assign used/disabled sprite in Inspector

    private const float buttonPressScale = 0.9f;
    private const float buttonAnimationDuration = 0.1f;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene!");
            return;
        }

        if (trashAndRefillButton != null)
        {
            trashAndRefillButton.onClick.AddListener(() => StartCoroutine(AnimateButtonPress(trashAndRefillButton)));
            trashAndRefillButton.onClick.AddListener(OnTrashAndRefillButtonClicked);
        }
        if (declareStopButton != null)
        {
            declareStopButton.onClick.AddListener(() => StartCoroutine(AnimateButtonPress(declareStopButton)));
            declareStopButton.onClick.AddListener(OnDeclareStopButtonClicked);
        }
        if (jokboButton != null)
        {
            jokboButton.onClick.AddListener(() => StartCoroutine(AnimateButtonPress(jokboButton)));
            jokboButton.onClick.AddListener(ToggleJokboPanel);
        }

        if (jokboPanel != null)
        {
            jokboPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }

    void OnRestartButtonClicked()
    {
        if (gameManager != null)
        {
            gameManager.RequestRestartGameServerRpc();
        }
    }

    public void ShowRestartButton()
    {
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    void ToggleJokboPanel()
    {
        if (jokboPanel != null)
        {
            jokboPanel.SetActive(!jokboPanel.activeSelf);
        }
    }

    public void UpdateButtonStates(GameManager.GameState currentState, Player currentPlayer)
    {
        Debug.Log("UpdateButtonStates called. CurrentState: " + currentState + ", CurrentPlayer: " + (currentPlayer != null ? currentPlayer.playerName : "None")); // Added
        bool isPlaying = currentState == GameManager.GameState.Playing;

        // Trash and Refill button logic
        // Check if it's the current player's turn AND if that current player is the local player
        bool isLocalPlayersTurn = (currentPlayer != null && currentPlayer.IsOwner); // New condition
        bool canUseTrashAndRefill = isPlaying && isLocalPlayersTurn && !currentPlayer.hasUsedTrashAndRefill.Value; // Modified
        Debug.Log("canUseTrashAndRefill: " + canUseTrashAndRefill); // Added

        if (trashAndRefillButton != null)
        {
            Debug.Log("Button Image component found: " + (trashAndRefillButton.GetComponent<Image>() != null)); // Added
            trashAndRefillButton.interactable = canUseTrashAndRefill;

            // Visual feedback for used state
            Image buttonImage = trashAndRefillButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (canUseTrashAndRefill)
                {
                    buttonImage.sprite = trashAndRefillNormalSprite;
                }
                else
                {
                    Debug.Log("Attempting to set used sprite. Sprite is null: " + (trashAndRefillUsedSprite == null)); // Added
                    buttonImage.sprite = trashAndRefillUsedSprite;
                }
            }
        }

        // Declare Stop button logic
        if (declareStopButton != null)
        {
            declareStopButton.interactable = isPlaying && (currentPlayer != null && gameManager.players[gameManager.currentPlayerIndex] == currentPlayer);
        }
    }

    public void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    void OnTrashAndRefillButtonClicked()
    {
        Debug.Log("Trash and Refill Button Clicked!");
        gameManager.RequestTrashAndRefillServerRpc();
    }

    void OnDeclareStopButtonClicked()
    {
        Debug.Log("Declare Stop Button Clicked!");
        gameManager.RequestDeclareStopServerRpc();
    }

    private IEnumerator AnimateButtonPress(Button button)
    {
        Vector3 originalScale = button.transform.localScale;
        Vector3 pressedScale = originalScale * buttonPressScale;

        float timer = 0f;
        while (timer < buttonAnimationDuration)
        {
            timer += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(originalScale, pressedScale, timer / buttonAnimationDuration);
            yield return null;
        }
        button.transform.localScale = pressedScale;

        timer = 0f;
        while (timer < buttonAnimationDuration)
        {
            timer += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(pressedScale, originalScale, timer / buttonAnimationDuration);
            yield return null;
        }
        button.transform.localScale = originalScale;
    }
}
