using Unity.Netcode;
using UnityEngine;

public class ClientConnection : SingletonNetwork<ClientConnection>
{
    [SerializeField]
    private int m_maxConnections;

    [SerializeField]
    private CharacterDataSO[] m_characterDatas;

    // This is a check for some script that depends on the client where it leaves
    // to check if this was a client that should no be allowed an there for the code should not run
    public bool IsExtraClient(ulong clientId)
    {
        return CanConnect(clientId);
    }

    // Check if a client can connect to the scene, this is call on every load of a scene.
    public bool CanClientConnect(ulong clientId)
    {
        if (!IsServer)
            return false;

        // Check if the client can connect
        bool canConnect = CanConnect(clientId);
        if (!canConnect)
        {
            RemoveClient(clientId);
        }

        return canConnect;
    }

    // Check if client can connect, there are two different ways to check
    // 1. On the selection screen, always check the network manager for the numbers of clients connected
    // 2. When the game starts after the character selection a new client should never be allowed to enter
    //    so we check the data of the characters because there we now witch character is selected and by who
    private bool CanConnect(ulong clientId)
    {
        if (LoadingSceneManager.Instance.SceneActive == SceneName.CharacterSelection)
        {
            int playersConnected = NetworkManager.Singleton.ConnectedClientsList.Count;

            if (playersConnected > m_maxConnections)
            {
                print($"Sorry we are full {clientId}");
                return false;
            }

            print($"You are allowed to enter {clientId}");
            return true;
        }
        else
        {
            if (ItHasACharacterSelected(clientId))
            {
                print($"You are allowed to enter {clientId}");
                return true;
            }
            else
            {
                print($"Sorry we are full {clientId}");
                return false;
            }
        }
    }

    // In case the client is not allowed to enter, remove the client for the session
    private void RemoveClient(ulong clientId)
    {
        // Client should shutdown
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        // Only send the rpc to the client that will be shutdown
        ShutdownClientRpc(clientRpcParams);
    }

    // Check if the client exist on the characters data
    private bool ItHasACharacterSelected(ulong clientId)
    {
        foreach (var data in m_characterDatas)
        {
            if (data.clientId == clientId)
            {
                return true;
            }
        }

        return false;
    }

    [ClientRpc]
    private void ShutdownClientRpc(ClientRpcParams clientRpcParams = default)
    {        
        Shutdown();
    }

    private void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }
}