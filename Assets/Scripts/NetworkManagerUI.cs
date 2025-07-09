using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private GameObject buttonsPanel; // New: Reference to the GameObject containing the buttons

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
    }

    void Start()
    {
        // Subscribe to events that indicate a successful connection/start
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HideButtonsOnConnected;
            NetworkManager.Singleton.OnServerStarted += HideButtonsOnConnected;
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null in NetworkManagerUI.Start(). Make sure NetworkManager GameObject is in the scene and active.");
        }
    }

    private void HideButtonsOnConnected(ulong clientId) // For OnClientConnectedCallback
    {
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Buttons Panel is not assigned in NetworkManagerUI. Cannot hide buttons.");
        }
        // Unsubscribe to prevent multiple calls if this script persists
        NetworkManager.Singleton.OnClientConnectedCallback -= HideButtonsOnConnected;
    }

    private void HideButtonsOnConnected() // For OnServerStarted (overload)
    {
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Buttons Panel is not assigned in NetworkManagerUI. Cannot hide buttons.");
        }
        // Unsubscribe to prevent multiple calls if this script persists
        NetworkManager.Singleton.OnServerStarted -= HideButtonsOnConnected;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HideButtonsOnConnected;
            NetworkManager.Singleton.OnServerStarted -= HideButtonsOnConnected;
        }
    }
}
