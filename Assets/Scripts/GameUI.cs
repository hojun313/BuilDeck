
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public GameManager gameManager;

    public Button trashAndRefillButton;
    public Button declareStopButton;
    public TextMeshProUGUI statusText; // 상태 메시지를 표시할 텍스트

    void Start()
    {
        // GameManager 참조를 자동으로 찾거나, Inspector에서 연결하도록 합니다.
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene!");
            return;
        }

        // 버튼 클릭 이벤트 리스너 추가
        if (trashAndRefillButton != null)
        {
            trashAndRefillButton.onClick.AddListener(OnTrashAndRefillButtonClicked);
        }
        if (declareStopButton != null)
        {
            declareStopButton.onClick.AddListener(OnDeclareStopButtonClicked);
        }
    }

    public void UpdateButtonStates(GameManager.GameState currentState)
    {
        bool isPlaying = currentState == GameManager.GameState.Playing;

        if (trashAndRefillButton != null) 
        {
            trashAndRefillButton.interactable = isPlaying;
        }
        if (declareStopButton != null) 
        {
            declareStopButton.interactable = isPlaying;
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
        gameManager.TrashAndRefillFieldDeck();
    }

    void OnDeclareStopButtonClicked()
    {
        Debug.Log("Declare Stop Button Clicked!");
        gameManager.DeclareStop();
    }
}
