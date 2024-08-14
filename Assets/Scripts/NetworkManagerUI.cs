using Unity.Netcode;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    private void OnGUI()
    {
        if (GUILayout.Button("Host"))
        {
            networkManager.StartHost();
            GameObject.Find("NetworkObjectPool").GetComponent<NetworkObjectPool>().InitializePool();
        }

        if (GUILayout.Button("Join"))
        {
            networkManager.StartClient();
            GameObject.Find("NetworkObjectPool").GetComponent<NetworkObjectPool>().InitializePool();
        }
        if (GUILayout.Button("Quit"))
        {
            Application.Quit(); 
        }
    }
}
