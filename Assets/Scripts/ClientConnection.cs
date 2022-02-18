using Unity.Netcode;
using UnityEngine;

public class ClientConnection : SingletonNetwork<ClientConnection>
{
    [SerializeField]
    private int m_maxConnections;

    [SerializeField]
    private CharacterDataSO[] m_characterDatas;    

    [ClientRpc]
    void ShutdownClientRpc(ClientRpcParams clientRpcParams = default)
    {        
        Shutdown();
    }
    
    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }

    // Check if the client exist on the characters data
    bool ItHasACharacterSelected(ulong clientId)
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

    // In case the client is not allowed to enter, remove the client for the session
    void RemoveClient(ulong clientId)
    {
        // Client should shutdown
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        // Only send the rpc to the client that wiil be shutdown
        ShutdownClientRpc(clientRpcParams);
    }

    // Check if client can connect, there two diferents ways to check
    // 1. on the selection screen always check the network manager for the numbers of clients connencted
    // 2. when the game start after the character selection a new client should never be allowed to enter
    // so we check the data of the characters beacause there we now witch character is selected and by who
    bool CanConnect(ulong clientId)
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

    // This is a check for some script that depend on the client where it leaves 
    // to check if this was a client that should no be allow an there for the code should no run
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
}
