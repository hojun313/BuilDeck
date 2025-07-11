using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ResultUI : MonoBehaviour
{
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button backToLobbyButton;

    void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (backToLobbyButton != null)
        {
            backToLobbyButton.onClick.AddListener(OnBackToLobbyClicked);
        }
    }

    public void ShowResult(List<Player> players)
    {
        if (resultPanel == null || resultText == null)
        {
            Debug.LogError("ResultUI is not properly set up!");
            return;
        }

        // 점수 순으로 플레이어 정렬
        var sortedPlayers = players.OrderByDescending(p => p.EvaluateHand().Rank)
                                   .ThenByDescending(p => p.EvaluateHand().HighCardRanks[0])
                                   .ThenByDescending(p => p.EvaluateHand().HighCardRanks.Count > 1 ? p.EvaluateHand().HighCardRanks[1] : 0)
                                   .ThenByDescending(p => p.EvaluateHand().HighCardRanks.Count > 2 ? p.EvaluateHand().HighCardRanks[2] : 0)
                                   .ThenByDescending(p => p.EvaluateHand().HighCardRanks.Count > 3 ? p.EvaluateHand().HighCardRanks[3] : 0)
                                   .ThenByDescending(p => p.EvaluateHand().HighCardRanks.Count > 4 ? p.EvaluateHand().HighCardRanks[4] : 0)
                                   .ToList();

        string resultString = "<align=\"center\"><b>Game Over!</b></align>\n\n";
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            Player player = sortedPlayers[i];
            Player.HandEvaluationResult result = player.EvaluateHand();
            resultString += $"{i + 1}. {player.playerName} - {result.Rank}\n";
        }

        resultText.text = resultString;
        resultPanel.SetActive(true);
    }

    void OnBackToLobbyClicked()
    {
        // 네트워크 연결 해제 및 로비 씬으로 돌아가는 로직 추가
        // 예: NetworkManager.Singleton.Shutdown();
        // UnityEngine.SceneManagement.SceneManager.LoadScene("LobbySceneName");
        Debug.Log("Back to Lobby button clicked. Implement scene transition.");
    }
}
