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
    [SerializeField] private GameUI gameUI; // Reference to the GameUI script

    void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            if (gameUI == null) { Debug.LogError("GameUI is not assigned in NetworkManagerUI!"); return; }
            NetworkManager.Singleton.StartHost();
            buttonsPanel.SetActive(false);
            gameUIPanel.SetActive(true);
            gameUI.UpdateStatusText("Waiting for opponent...");
        });

        clientButton.onClick.AddListener(() =>
        {
            if (gameUI == null) { Debug.LogError("GameUI is not assigned in NetworkManagerUI!"); return; }
            NetworkManager.Singleton.StartClient();
            buttonsPanel.SetActive(false);
            gameUIPanel.SetActive(true);
            gameUI.UpdateStatusText("Waiting for opponent...");
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
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null in NetworkManagerUI.Start(). Make sure NetworkManager GameObject is in the scene and active.");
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        // This is called on the client when a connection is established.
        // The game UI is already visible, so we might not need to do anything here,
        // as the GameManager will likely update the status text.
    }

    private void HandleServerStarted()
    {
        // This is called on the host/server when it starts.
        // The game UI is already visible.
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
        }
    }
}
