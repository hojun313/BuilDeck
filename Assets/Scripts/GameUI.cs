
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public GameManager gameManager;

    public Button swapCardButton;
    public Button trashAndRefillButton;
    public Button declareStopButton;

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
        if (swapCardButton != null)
        {
            swapCardButton.onClick.AddListener(OnSwapCardButtonClicked);
        }
        if (trashAndRefillButton != null)
        {
            trashAndRefillButton.onClick.AddListener(OnTrashAndRefillButtonClicked);
        }
        if (declareStopButton != null)
        {
            declareStopButton.onClick.AddListener(OnDeclareStopButtonClicked);
        }
    }

    void OnSwapCardButtonClicked()
    {
        // 테스트를 위해 임의의 카드 인덱스를 사용합니다.
        // 실제 게임에서는 플레이어가 선택한 카드 인덱스를 받아와야 합니다.
        int playerCardIndex = 0; // 플레이어 핸드의 첫 번째 카드
        int fieldCardIndex = 0;  // 필드 덱의 첫 번째 카드

        Debug.Log("Swap Card Button Clicked!");
        gameManager.SwapCardWithFieldDeck(playerCardIndex, fieldCardIndex);
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
