
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Coroutine을 위해 추가

public class GameUI : MonoBehaviour
{
    public GameManager gameManager;

    public Button trashAndRefillButton;
    public Button declareStopButton;
    public TextMeshProUGUI statusText; // 상태 메시지를 표시할 텍스트

    private const float buttonPressScale = 0.9f; // 버튼이 눌렸을 때의 스케일
    private const float buttonAnimationDuration = 0.1f; // 버튼 애니메이션 지속 시간

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
            trashAndRefillButton.onClick.AddListener(() => StartCoroutine(AnimateButtonPress(trashAndRefillButton)));
            trashAndRefillButton.onClick.AddListener(OnTrashAndRefillButtonClicked);
        }
        if (declareStopButton != null)
        {
            declareStopButton.onClick.AddListener(() => StartCoroutine(AnimateButtonPress(declareStopButton)));
            declareStopButton.onClick.AddListener(OnDeclareStopButtonClicked);
        }
    }

    public void UpdateButtonStates(GameManager.GameState currentState, Player currentPlayer)
    {
        bool isPlaying = currentState == GameManager.GameState.Playing;

        // Trash and Refill 버튼은 현재 플레이어의 턴에만 활성화
        if (trashAndRefillButton != null) 
        {
            trashAndRefillButton.interactable = isPlaying && (currentPlayer != null && gameManager.players[gameManager.currentPlayerIndex] == currentPlayer);
        }
        // Declare Stop 버튼은 현재 플레이어의 턴에만 활성화
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
        gameManager.TrashAndRefillFieldDeck();
    }

    void OnDeclareStopButtonClicked()
    {
        Debug.Log("Declare Stop Button Clicked!");
        gameManager.DeclareStop();
    }

    private IEnumerator AnimateButtonPress(Button button)
    {
        Vector3 originalScale = button.transform.localScale;
        Vector3 pressedScale = originalScale * buttonPressScale;

        // 버튼 누르는 애니메이션
        float timer = 0f;
        while (timer < buttonAnimationDuration)
        {
            timer += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(originalScale, pressedScale, timer / buttonAnimationDuration);
            yield return null;
        }
        button.transform.localScale = pressedScale; // 정확히 눌린 스케일로 설정

        // 버튼 원래대로 돌아오는 애니메이션
        timer = 0f;
        while (timer < buttonAnimationDuration)
        {
            timer += Time.deltaTime;
            button.transform.localScale = Vector3.Lerp(pressedScale, originalScale, timer / buttonAnimationDuration);
            yield return null;
        }
        button.transform.localScale = originalScale; // 정확히 원래 스케일로 설정
    }
}
