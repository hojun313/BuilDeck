using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private GameObject buttonsPanel; // Reference to the GameObject containing the host/client/server buttons
    [SerializeField] private GameObject gameUIPanel; // New: Reference to the GameObject containing the game UI (trash, stop buttons)

    void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });

        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });

        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });

        // Initially hide the game UI panel
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleConnected;
            NetworkManager.Singleton.OnServerStarted += HandleConnected;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null in NetworkManagerUI.Start(). Make sure NetworkManager GameObject is in the scene and active.");
        }
    }

    private void HandleConnected(ulong clientId) // For OnClientConnectedCallback
    {
        // Hide connection buttons
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Buttons Panel is not assigned in NetworkManagerUI. Cannot hide connection buttons.");
        }

        // Show game UI panel
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game UI Panel is not assigned in NetworkManagerUI. Cannot show game UI.");
        }

        // Unsubscribe to prevent multiple calls
        NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnected;
    }

    private void HandleConnected() // For OnServerStarted (overload)
    {
        // Hide connection buttons
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Buttons Panel is not assigned in NetworkManagerUI. Cannot hide connection buttons.");
        }

        // Show game UI panel
        if (gameUIPanel != null)
        {
            gameUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Game UI Panel is not assigned in NetworkManagerUI. Cannot show game UI.");
        }

        // Unsubscribe to prevent multiple calls
        NetworkManager.Singleton.OnServerStarted -= HandleConnected;
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleConnected;
            NetworkManager.Singleton.OnServerStarted -= HandleConnected;
        }
    }
}
